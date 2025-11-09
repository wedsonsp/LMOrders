namespace LMOrders.Application.Interfaces.Integrations;

public interface IKafkaProducerService
{
    Task ProduceAsync<T>(string topic, T message, CancellationToken cancellationToken = default);
}
