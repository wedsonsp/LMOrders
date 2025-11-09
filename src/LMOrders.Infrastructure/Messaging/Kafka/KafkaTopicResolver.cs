using LMOrders.Application.Interfaces.Integrations;
using LMOrders.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace LMOrders.Infrastructure.Messaging.Kafka;

public class KafkaTopicResolver : IKafkaTopicResolver
{
    private readonly KafkaOptions _options;

    public KafkaTopicResolver(IOptions<KafkaOptions> options)
    {
        _options = options.Value;
    }

    public string GetPedidoCriadoTopic()
    {
        return string.IsNullOrWhiteSpace(_options.PedidoCriadoTopic)
            ? "pedidos.criados"
            : _options.PedidoCriadoTopic;
    }
}
