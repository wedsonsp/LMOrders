using LMOrders.Application.Events;
using LMOrders.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LMOrders.Infrastructure.Messaging.Kafka;

public class PedidoCriadoConsumerService : KafkaConsumerBackgroundService<PedidoCriadoEvent>
{
    private readonly ILogger<PedidoCriadoConsumerService> _logger;
    private readonly KafkaOptions _options;

    public PedidoCriadoConsumerService(
        IOptions<KafkaOptions> options,
        ILogger<PedidoCriadoConsumerService> logger)
        : base(options, logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override string Topic => _options.PedidoCriadoTopic;

    protected override Task ProcessMessageAsync(PedidoCriadoEvent message, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "PedidoCriado consumido. PedidoId: {PedidoId}, Cliente: {ClienteId}, ValorTotal: {ValorTotal}",
            message.PedidoId,
            message.ClienteId,
            message.ValorTotal);

        return Task.CompletedTask;
    }
}
