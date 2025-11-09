using System.Text.Json;
using Confluent.Kafka;
using LMOrders.Application.Interfaces.Integrations;
using LMOrders.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LMOrders.Infrastructure.Messaging.Kafka;

public class KafkaProducerService : IKafkaProducerService, IAsyncDisposable
{
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaProducerService> _logger;
    private IProducer<string, string>? _producer;

    public KafkaProducerService(IOptions<KafkaOptions> options, ILogger<KafkaProducerService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task ProduceAsync<T>(string topic, T message, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Kafka desabilitado. Mensagem para o tópico {Topic} não será publicada.", topic);
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.BootstrapServers))
        {
            _logger.LogWarning("BootstrapServers do Kafka não configurado. Mensagem para o tópico {Topic} descartada.", topic);
            return;
        }

        EnsureProducerCreated();

        var payload = JsonSerializer.Serialize(message);
        var kafkaMessage = new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = payload
        };

        try
        {
            var deliveryResult = await _producer!.ProduceAsync(topic, kafkaMessage, cancellationToken);
            _logger.LogInformation("Evento publicado no Kafka. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}", deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Falha ao publicar mensagem no tópico {Topic}.", topic);
            throw;
        }
    }

    private void EnsureProducerCreated()
    {
        if (_producer is not null)
        {
            return;
        }

        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            EnableDeliveryReports = true
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public ValueTask DisposeAsync()
    {
        _producer?.Flush(TimeSpan.FromSeconds(2));
        _producer?.Dispose();
        return ValueTask.CompletedTask;
    }
}
