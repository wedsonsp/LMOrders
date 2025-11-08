using LMOrders.Domain.Entities;

namespace LMOrders.Application.Interfaces.Integrations;

public interface IBillingIntegrationService
{
    Task NotifyAsync(Pedido pedido, CancellationToken cancellationToken = default);
}

