using System.Text.Json;
using Confluent.Kafka;
using LMOrders.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LMOrders.Infrastructure.Messaging.Kafka;

public abstract class KafkaConsumerBackgroundService<TMessage> : BackgroundService
{
    private readonly KafkaOptions _options;
    private readonly ILogger _logger;

    protected KafkaConsumerBackgroundService(IOptions<KafkaOptions> options, ILogger logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected abstract string Topic { get; }
    protected abstract Task ProcessMessageAsync(TMessage message, CancellationToken cancellationToken);

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Kafka desabilitado. Consumer {Consumer} não será iniciado.", GetType().Name);
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(_options.BootstrapServers) || string.IsNullOrWhiteSpace(_options.ConsumerGroupId))
        {
            _logger.LogWarning("Configurações do Kafka inválidas. Consumer {Consumer} não será iniciado.", GetType().Name);
            return Task.CompletedTask;
        }

        return Task.Run(() => ConsumeLoopAsync(stoppingToken), stoppingToken);
    }

    private async Task ConsumeLoopAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(Topic);

        _logger.LogInformation("Consumer {Consumer} inscrito no tópico {Topic}.", GetType().Name, Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);

                    if (result?.Message?.Value is null)
                    {
                        continue;
                    }

                    var message = JsonSerializer.Deserialize<TMessage>(result.Message.Value);

                    if (message is not null)
                    {
                        await ProcessMessageAsync(message, stoppingToken);
                    }

                    consumer.Commit(result);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Erro ao consumir mensagem do Kafka.");
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Erro ao desserializar mensagem do Kafka.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro inesperado ao processar mensagem do Kafka.");
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }
}
