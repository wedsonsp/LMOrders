namespace LMOrders.Infrastructure.Options;

public class KafkaOptions
{
    public const string SectionName = "Kafka";

    public bool Enabled { get; init; } = false;
    public string BootstrapServers { get; init; } = string.Empty;
    public string PedidoCriadoTopic { get; init; } = "pedidos.criados";
    public string ConsumerGroupId { get; init; } = "lmorders-consumers";
}
