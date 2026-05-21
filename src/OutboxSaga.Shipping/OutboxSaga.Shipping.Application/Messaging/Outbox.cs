using System.Text.Json;

namespace OutboxSaga.Shipping.Application.Messaging;

public sealed class OutboxMessage
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string AggregateId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public string? CorrelationId { get; init; }
    public string? CausationId { get; init; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime? PublishedAtUtc { get; private set; }

    public static OutboxMessage FromIntegrationEvent<T>(T integrationEvent) where T : OutboxSaga.Messaging.IntegrationEvent
    {
        return new OutboxMessage
        {
            AggregateId = integrationEvent.Id,
            EventType = typeof(T).FullName ?? typeof(T).Name,
            Payload = JsonSerializer.Serialize(integrationEvent, typeof(T), SerializerOptions),
            CorrelationId = integrationEvent.CorrelationId,
            CausationId = integrationEvent.CausationId
        };
    }

    public void MarkAsPublished(DateTime publishedAtUtc)
    {
        PublishedAtUtc = publishedAtUtc;
    }
}

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken ct = default);
    Task<IReadOnlyList<OutboxMessage>> GetUnpublishedAsync(int batchSize, CancellationToken ct = default);
    Task MarkAsPublishedAsync(string messageId, DateTime publishedAtUtc, CancellationToken ct = default);
}
