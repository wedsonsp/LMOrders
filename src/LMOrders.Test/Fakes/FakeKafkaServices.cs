using System.Collections.Generic;
using LMOrders.Application.Interfaces.Integrations;

namespace LMOrders.Test.Fakes;

public class FakeKafkaProducerService : IKafkaProducerService
{
    public List<(string Topic, object Message)> ProducedMessages { get; } = new();

    public Task ProduceAsync<T>(string topic, T message, CancellationToken cancellationToken = default)
    {
        ProducedMessages.Add((topic, message!));
        return Task.CompletedTask;
    }
}

public class FakeKafkaTopicResolver : IKafkaTopicResolver
{
    public string GetPedidoCriadoTopic() => "test-pedidos-criados";
}
