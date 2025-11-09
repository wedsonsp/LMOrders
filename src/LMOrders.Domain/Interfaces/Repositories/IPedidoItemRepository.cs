using LMOrders.Domain.Entities;

namespace LMOrders.Domain.Interfaces.Repositories;

public interface IPedidoItemRepository
{
    Task SalvarItensAsync(int pedidoId, IEnumerable<PedidoItem> itens, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PedidoItem>> ObterPorPedidoIdAsync(int pedidoId, CancellationToken cancellationToken = default);
    Task RemoverPorPedidoIdAsync(int pedidoId, CancellationToken cancellationToken = default);
}
