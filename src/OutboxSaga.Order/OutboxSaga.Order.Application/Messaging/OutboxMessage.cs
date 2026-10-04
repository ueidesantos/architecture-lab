using System.Text.Json;
using OutboxSaga.Orders.Domain.Common;

namespace OutboxSaga.Orders.Application.Messaging;

public sealed class OutboxMessage
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string AggregateId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;

    /// <summary>
    /// Trace ID that connects all messages in a single business transaction.
    /// Essential for distributed tracing and observability in Sagas.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// The ID of the message that caused this one.
    /// Useful for debugging the flow of events (who triggered what).
    /// </summary>
    public string? CausationId { get; init; }

    public DateTime OccurredOnUtc { get; init; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime? PublishedAtUtc { get; private set; }

    public static OutboxMessage FromDomainEvent(
        string aggregateId,
        IDomainEvent domainEvent,
        string? correlationId = null,
        string? causationId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(aggregateId);
        ArgumentNullException.ThrowIfNull(domainEvent);

        return new OutboxMessage
        {
            AggregateId = aggregateId,
            EventType = domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), SerializerOptions),
            OccurredOnUtc = domainEvent.OccurredOnUtc,
            CorrelationId = correlationId,
            CausationId = causationId
        };
    }

    public void MarkAsPublished(DateTime publishedAtUtc)
    {
        PublishedAtUtc = publishedAtUtc;
    }
}
