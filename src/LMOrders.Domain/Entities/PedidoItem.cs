using LMOrders.Domain.Exceptions;

namespace LMOrders.Domain.Entities;

public class PedidoItem : BaseEntity
{
    private PedidoItem()
    {
        Produto = string.Empty;
    }

    public PedidoItem(int produtoId, string produto, int quantidade, decimal valorUnitario)
    {
        AtualizarProduto(produtoId, produto);
        AtualizarPreco(quantidade, valorUnitario);
    }

    public int PedidoId { get; private set; }
    public int ProdutoId { get; private set; }
    public string Produto { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public decimal Total => Quantidade * ValorUnitario;

    public void AssociarPedido(int pedidoId)
    {
        if (pedidoId <= 0)
        {
            throw new DomainException("O identificador do pedido é obrigatório.");
        }

        PedidoId = pedidoId;
        Touch();
    }

    public void AtualizarProduto(int produtoId, string produto)
    {
        if (produtoId <= 0)
        {
            throw new DomainException("O identificador do produto é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(produto))
        {
            throw new DomainException("O nome do produto é obrigatório.");
        }

        ProdutoId = produtoId;
        Produto = produto.Trim();
        Touch();
    }

    public void AtualizarPreco(int quantidade, decimal valorUnitario)
    {
        if (quantidade <= 0)
        {
            throw new DomainException("A quantidade deve ser maior que zero.");
        }

        if (valorUnitario <= 0)
        {
            throw new DomainException("O preço unitário deve ser maior que zero.");
        }

        Quantidade = quantidade;
        ValorUnitario = decimal.Round(valorUnitario, 2, MidpointRounding.AwayFromZero);
        Touch();
    }
}

