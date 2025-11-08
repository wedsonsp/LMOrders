using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LMOrders.Infrastructure.Messaging;

public class BillingMessageDispatcher : BackgroundService
{
    private readonly ChannelReader<BillingMessage> _channelReader;
    private readonly ILogger<BillingMessageDispatcher> _logger;

    public BillingMessageDispatcher(
        ChannelReader<BillingMessage> channelReader,
        ILogger<BillingMessageDispatcher> logger)
    {
        _channelReader = channelReader;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _channelReader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation(
                    "Mensagem de faturamento recebida. Pedido: {PedidoId}, Total: {Total:C2}",
                    message.PedidoId,
                    message.ValorTotal);

                // Simula chamada assíncrona ao sistema externo
                await Task.Delay(TimeSpan.FromMilliseconds(150), stoppingToken);

                _logger.LogInformation(
                    "Pedido {PedidoId} enviado para faturamento às {DateTimeUtc:O}",
                    message.PedidoId,
                    DateTime.UtcNow);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Processamento de mensagem cancelado.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem de faturamento para o pedido {PedidoId}.", message.PedidoId);
            }
        }
    }
}

