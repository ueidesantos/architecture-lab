using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Text.Json;

namespace OutboxSaga.Orders.Infrastructure.Outbox;

/// <summary>
/// Staff Architect Note:
/// This service implements the "Publisher" part of the Outbox Pattern.
/// It ensures "At Least Once" delivery by polling the database and publishing to Kafka.
///
/// Patterns applied:
/// - Batching: Efficiently processes multiple messages in one loop.
/// - Exponential Backoff (Polly): Resilience against transient failures in the Broker.
/// - Observability: Detailed logging of message lifecycle.
/// </summary>
public interface IIntegrationEventPublisher
{
    Task PublishAsync(string topic, string key, string payload, CancellationToken ct = default);
}

public sealed class KafkaIntegrationEventPublisher : IIntegrationEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaIntegrationEventPublisher> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public KafkaIntegrationEventPublisher(
        string bootstrapServers,
        ILogger<KafkaIntegrationEventPublisher> logger)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All, // Staff Architect: Maximum reliability
            EnableIdempotence = true, // Prevent duplicates at the broker level
            MessageSendMaxRetries = 3
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
        _logger = logger;

        // Resilience policy for transient errors
        _retryPolicy = Policy
            .Handle<KafkaException>()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception, "Transient error publishing to Kafka. Retrying in {TimeSpan} (Attempt {RetryCount})", timeSpan, retryCount);
                });
    }

    public async Task PublishAsync(string topic, string key, string payload, CancellationToken ct = default)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var message = new Message<string, string>
            {
                Key = key,
                Value = payload
            };

            var result = await _producer.ProduceAsync(topic, message, ct);

            _logger.LogInformation("Message delivered to {Topic} at offset {Offset}",
                result.TopicPartitionOffset.Topic,
                result.TopicPartitionOffset.Offset);
        });
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}
