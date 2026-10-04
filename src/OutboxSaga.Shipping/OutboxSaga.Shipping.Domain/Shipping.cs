namespace OutboxSaga.Shipping.Domain;

public enum ShippingStatus
{
    Pending,
    Shipped,
    Delivered,
    Cancelled
}

public class Shipping
{
    public string Id { get; private set; } = Guid.NewGuid().ToString("N");
    public string OrderId { get; private set; } = string.Empty;
    public string TrackingCode { get; private set; } = string.Empty;
    public ShippingStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime EstimatedDelivery { get; private set; }

    private Shipping() { }

    public static Shipping Create(string orderId)
    {
        return new Shipping
        {
            OrderId = orderId,
            Status = ShippingStatus.Pending,
            TrackingCode = $"TRK-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            EstimatedDelivery = DateTime.UtcNow.AddDays(5)
        };
    }

    public void Ship()
    {
        Status = ShippingStatus.Shipped;
    }
}
