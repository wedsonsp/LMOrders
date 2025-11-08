namespace LMOrders.Application.DTOs;

public class CriarPedidoRequest
{
    public int ClienteId { get; set; }
    public DateTime DataPedido { get; set; }
    public IList<CriarPedidoItemDto> Itens { get; set; } = new List<CriarPedidoItemDto>();
}

