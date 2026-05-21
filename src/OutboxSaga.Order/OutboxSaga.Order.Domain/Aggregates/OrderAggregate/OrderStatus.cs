namespace OutboxSaga.Orders.Domain.Aggregates.OrderAggregate;

public sealed record OrderStatus
{
    public static readonly OrderStatus Creating = new("Creating");
    public static readonly OrderStatus Created = new("Created");
    public static readonly OrderStatus PendingPayment = new("PendingPayment");
    public static readonly OrderStatus ProcessingPayment = new("ProcessingPayment");
    public static readonly OrderStatus PendingShipment = new("PendingShipment");
    public static readonly OrderStatus Shipped = new("Shipped");
    public static readonly OrderStatus Cancelled = new("Cancelled");

    private static readonly IReadOnlyDictionary<string, OrderStatus> KnownStatuses =
        new[]
        {
            Creating,
            Created,
            PendingPayment,
            ProcessingPayment,
            PendingShipment,
            Shipped,
            Cancelled
        }.ToDictionary(status => status.Value);

    private OrderStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static OrderStatus From(string value)
    {
        if (KnownStatuses.TryGetValue(value, out var status))
        {
            return status;
        }

        throw new ArgumentException($"Unknown order status '{value}'.", nameof(value));
    }

    public override string ToString() => Value;
}
