namespace LMOrders.Application.DTOs;

public class PedidoItemDto
{
    public int ProdutoId { get; set; }
    public string Produto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Total { get; set; }
}

