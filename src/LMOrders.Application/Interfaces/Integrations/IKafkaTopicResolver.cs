namespace LMOrders.Application.Interfaces.Integrations;

public interface IKafkaTopicResolver
{
    string GetPedidoCriadoTopic();
}
