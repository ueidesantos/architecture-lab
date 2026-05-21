using OutboxSaga.Orders.Domain.Common;
using OutboxSaga.Orders.Domain.ValueObjects;

namespace OutboxSaga.Orders.Domain.Events;

public sealed record OrderCreatedDomainEvent(
    string OrderId,
    string CustomerId,
    Money TotalValue,
    DateTime OccurredOnUtc) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
}
