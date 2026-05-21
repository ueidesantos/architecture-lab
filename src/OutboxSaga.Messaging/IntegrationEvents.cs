namespace OutboxSaga.Messaging;

/// <summary>
/// Staff Architect Note:
/// Integration events are used to communicate between microservices.
/// They must contain enough information for the consumer to act,
/// but should not leak internal domain details.
/// </summary>
public abstract record IntegrationEvent
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Identifies the sequence of events.
    /// </summary>
    public string CorrelationId { get; init; } = string.Empty;

    /// <summary>
    /// Identifies the message that triggered this event.
    /// </summary>
    public string CausationId { get; init; } = string.Empty;
}

public record OrderCreatedIntegrationEvent : IntegrationEvent
{
    public string OrderId { get; init; } = string.Empty;
    public string CustomerId { get; init; } = string.Empty;
    public decimal TotalValue { get; init; }
    public string Currency { get; init; } = "BRL";
}

public record PaymentProcessedIntegrationEvent : IntegrationEvent
{
    public string PaymentId { get; init; } = Guid.NewGuid().ToString("N");
    public string OrderId { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string? Reason { get; init; }
}

public record ShippingArrangedIntegrationEvent : IntegrationEvent
{
    public string ShippingId { get; init; } = Guid.NewGuid().ToString("N");
    public string OrderId { get; init; } = string.Empty;
    public string TrackingCode { get; init; } = string.Empty;
    public DateTime EstimatedDelivery { get; init; }
}
