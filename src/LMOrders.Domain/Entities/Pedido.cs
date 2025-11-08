using LMOrders.Domain.Enums;
using LMOrders.Domain.Exceptions;

namespace LMOrders.Domain.Entities;

public class Pedido : BaseEntity
{
    private readonly List<PedidoItem> _itens = new();

    private Pedido()
    {
        Status = PedidoStatus.Pendente;
        CreatedAt = DateTime.UtcNow;
    }

    public Pedido(int clienteId, DateTime dataPedido, IEnumerable<PedidoItem> itens)
    {
        DefinirCliente(clienteId);
        DefinirData(dataPedido);
        Status = PedidoStatus.Pendente;

        foreach (var item in itens)
        {
            AdicionarItem(item);
        }
    }

    public int ClienteId { get; private set; }
    public DateTime DataPedido { get; private set; }
    public PedidoStatus Status { get; private set; }
    public IReadOnlyCollection<PedidoItem> Itens => _itens.AsReadOnly();
    public decimal ValorTotal => _itens.Sum(item => item.Total);

    public void DefinirCliente(int clienteId)
    {
        if (clienteId <= 0)
        {
            throw new DomainException("O ClienteId é obrigatório.");
        }

        ClienteId = clienteId;
    }

    public void DefinirData(DateTime dataPedido)
    {
        if (dataPedido == default)
        {
            throw new DomainException("A data do pedido é obrigatória.");
        }

        DataPedido = dataPedido.ToUniversalTime();
    }

    public void AdicionarItem(PedidoItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _itens.Add(item);
    }

    public void AtualizarStatus(PedidoStatus status)
    {
        Status = status;
        Touch();
    }
}

