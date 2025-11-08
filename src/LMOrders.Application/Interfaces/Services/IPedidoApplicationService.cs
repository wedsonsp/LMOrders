using LMOrders.Application.DTOs;

namespace LMOrders.Application.Interfaces.Services;

public interface IPedidoApplicationService
{
    Task<PedidoResponse> CriarAsync(CriarPedidoRequest request, CancellationToken cancellationToken = default);
    Task<PedidoResponse?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
}

