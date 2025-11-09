namespace LMOrders.Application.Events;

public record PedidoCriadoItemEvent(int ProdutoId, string Produto, int Quantidade, decimal ValorUnitario);

public record PedidoCriadoEvent(
    int PedidoId,
    int ClienteId,
    DateTime DataPedido,
    decimal ValorTotal,
    IReadOnlyCollection<PedidoCriadoItemEvent> Itens);
