using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OutboxSaga.Orders.Application.Abstractions.Messaging;

namespace OutboxSaga.Orders.Infrastructure.Outbox;

/// <summary>
/// Staff Architect Note:
/// BackgroundService that orchestrates the Outbox message publication.
///
/// Why a BackgroundService?
/// It detaches the message publication from the API request, ensuring the API stays fast.
/// Even if Kafka is down, the business transaction is already safe in the Outbox table.
/// </summary>
public sealed class OutboxPublisherWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<OutboxPublisherWorker> _logger;
    private const int BatchSize = 20;
    private const string DefaultTopic = "orders-integration-events";

    public OutboxPublisherWorker(
        IServiceProvider serviceProvider,
        IIntegrationEventPublisher eventPublisher,
        ILogger<OutboxPublisherWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Publisher Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages.");
            }

            // Staff Architect: Configurable polling interval
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

        var messages = await repository.GetUnpublishedAsync(BatchSize, ct);

        if (messages.Count == 0) return;

        _logger.LogInformation("Processing {Count} outbox messages.", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                // In a real scenario, we might want to map the internal OutboxMessage
                // to a specific Integration Event based on message.EventType
                await _eventPublisher.PublishAsync(
                    topic: DefaultTopic,
                    key: message.AggregateId,
                    payload: message.Payload,
                    ct: ct);

                await repository.MarkAsPublishedAsync(message.Id, DateTime.UtcNow, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish outbox message {MessageId}", message.Id);
                // Staff Architect: Consider a "MaxRetries" or "Dead Letter" for the outbox if it keeps failing
            }
        }
    }
}
