using System.Text.Json;
using System.Threading.Channels;
using LMOrders.Application.Interfaces.Integrations;
using LMOrders.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LMOrders.Infrastructure.Messaging;

public class BillingIntegrationService : IBillingIntegrationService
{
    private readonly ChannelWriter<BillingMessage> _channelWriter;
    private readonly ILogger<BillingIntegrationService> _logger;

    public BillingIntegrationService(
        ChannelWriter<BillingMessage> channelWriter,
        ILogger<BillingIntegrationService> logger)
    {
        _channelWriter = channelWriter;
        _logger = logger;
    }

    public async Task NotifyAsync(Pedido pedido, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        var payload = JsonSerializer.Serialize(new
        {
            pedido.Id,
            pedido.ClienteId,
            pedido.DataPedido,
            pedido.Status,
            Itens = pedido.Itens.Select(item => new
            {
                item.ProdutoId,
                item.Produto,
                item.Quantidade,
                item.ValorUnitario,
                item.Total
            }),
            pedido.ValorTotal
        });

        var message = new BillingMessage(
            pedido.Id,
            pedido.ClienteId,
            pedido.ValorTotal,
            DateTime.UtcNow,
            payload);

        while (await _channelWriter.WaitToWriteAsync(cancellationToken)
               && !_channelWriter.TryWrite(message))
        {
            _logger.LogWarning("Falha ao enviar mensagem de faturamento. Tentando novamente.");
        }

        _logger.LogInformation("Pedido {PedidoId} preparado para faturamento assíncrono.", pedido.Id);
    }
}

