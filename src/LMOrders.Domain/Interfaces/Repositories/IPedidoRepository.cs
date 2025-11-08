using LMOrders.Domain.Entities;

namespace LMOrders.Domain.Interfaces.Repositories;

public interface IPedidoRepository
{
    Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken = default);
    Task<Pedido?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
}

