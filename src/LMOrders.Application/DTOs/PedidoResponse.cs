using LMOrders.Domain.Enums;

namespace LMOrders.Application.DTOs;

public class PedidoResponse
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public DateTime DataPedido { get; set; }
    public PedidoStatus Status { get; set; }
    public decimal ValorTotal { get; set; }
    public IList<PedidoItemDto> Itens { get; set; } = new List<PedidoItemDto>();
}

