namespace LMOrders.Application.DTOs;

public class CriarPedidoItemDto
{
    public int ProdutoId { get; set; }
    public string Produto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
}

