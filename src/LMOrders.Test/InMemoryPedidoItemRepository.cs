using System.Collections.Concurrent;
using LMOrders.Domain.Entities;
using LMOrders.Domain.Interfaces.Repositories;

namespace LMOrders.Test;

public class InMemoryPedidoItemRepository : IPedidoItemRepository
{
    private readonly ConcurrentDictionary<int, List<PedidoItem>> _storage = new();

    public Task SalvarItensAsync(int pedidoId, IEnumerable<PedidoItem> itens, CancellationToken cancellationToken = default)
    {
        var itensList = itens.Select(CloneItem).ToList();
        foreach (var item in itensList)
        {
            item.AssociarPedido(pedidoId);
        }

        _storage.AddOrUpdate(pedidoId, itensList, (_, _) => itensList);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<PedidoItem>> ObterPorPedidoIdAsync(int pedidoId, CancellationToken cancellationToken = default)
    {
        if (_storage.TryGetValue(pedidoId, out var itens))
        {
            var clones = itens.Select(CloneItem).ToList();
            foreach (var item in clones)
            {
                item.AssociarPedido(pedidoId);
            }

            return Task.FromResult<IReadOnlyCollection<PedidoItem>>(clones);
        }

        return Task.FromResult<IReadOnlyCollection<PedidoItem>>(Array.Empty<PedidoItem>());
    }

    public Task RemoverPorPedidoIdAsync(int pedidoId, CancellationToken cancellationToken = default)
    {
        _storage.TryRemove(pedidoId, out _);
        return Task.CompletedTask;
    }

    private static PedidoItem CloneItem(PedidoItem original)
    {
        var clone = new PedidoItem(original.ProdutoId, original.Produto, original.Quantidade, original.ValorUnitario);
        if (original.PedidoId > 0)
        {
            clone.AssociarPedido(original.PedidoId);
        }
        return clone;
    }
}
