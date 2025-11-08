namespace LMOrders.Infrastructure.Messaging;

public record BillingMessage(
    int PedidoId,
    int ClienteId,
    decimal ValorTotal,
    DateTime CriadoEm,
    string Payload);

