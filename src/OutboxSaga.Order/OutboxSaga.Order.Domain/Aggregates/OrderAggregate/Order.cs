using OutboxSaga.Orders.Domain.Common;
using OutboxSaga.Orders.Domain.Events;
using OutboxSaga.Orders.Domain.Exceptions;
using OutboxSaga.Orders.Domain.ValueObjects;

namespace OutboxSaga.Orders.Domain.Aggregates.OrderAggregate;

public sealed class Order : AggregateRoot<string>
{
    public Order(OrderCustomer customer, string description, decimal totalValue)
        : this(Guid.NewGuid().ToString("N"), customer, description, new Money(totalValue), DateTime.UtcNow)
    {
        RaiseDomainEvent(new OrderCreatedDomainEvent(
            Id,
            Customer.Id,
            TotalValue,
            CreatedAt));
    }

    private Order(
        string id,
        OrderCustomer customer,
        string description,
        Money totalValue,
        DateTime createdAt,
        OrderStatus? status = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOrderException("Order id is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new InvalidOrderException("Order description is required.");
        }

        Id = id;
        Customer = customer ?? throw new InvalidOrderException("Order customer is required.");
        Description = description.Trim();
        TotalValue = totalValue;
        CreatedAt = createdAt;
        Status = status ?? OrderStatus.Creating;
    }

    public static Order Rehydrate(
        string id,
        OrderCustomer customer,
        string description,
        Money totalValue,
        OrderStatus status,
        DateTime createdAt)
    {
        return new Order(id, customer, description, totalValue, createdAt, status);
    }

    public string Id { get; private set; }
    public OrderCustomer Customer { get; private set; }
    public string Description { get; private set; }
    public Money TotalValue { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void MarkAsCreated()
    {
        EnsureStatus(OrderStatus.Creating);

        Status = OrderStatus.Created;
    }

    public void MarkPaymentPending()
    {
        EnsureStatus(OrderStatus.Created);

        Status = OrderStatus.PendingPayment;
    }

    public void MarkPaymentProcessing()
    {
        EnsureStatus(OrderStatus.PendingPayment);

        Status = OrderStatus.ProcessingPayment;
    }

    public void MarkShipmentPending()
    {
        EnsureStatus(OrderStatus.ProcessingPayment);

        Status = OrderStatus.PendingShipment;
    }

    public void MarkAsShipped()
    {
        EnsureStatus(OrderStatus.PendingShipment);

        Status = OrderStatus.Shipped;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Shipped)
        {
            throw new InvalidOrderException("A shipped order cannot be cancelled.");
        }

        Status = OrderStatus.Cancelled;
    }

    private void EnsureStatus(OrderStatus expectedStatus)
    {
        if (Status != expectedStatus)
        {
            throw new InvalidOrderException(
                $"Order must be {expectedStatus.Value} but is {Status.Value}.");
        }
    }
}
