namespace LMOrders.Test.Models;

internal class PedidoResponseDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public DateTime DataPedido { get; set; }
    public decimal ValorTotal { get; set; }
    public PedidoItemDto[] Itens { get; set; } = Array.Empty<PedidoItemDto>();
}

internal class PedidoItemDto
{
    public int ProdutoId { get; set; }
    public string Produto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Total { get; set; }
}

