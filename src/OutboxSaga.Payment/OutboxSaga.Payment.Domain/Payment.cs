namespace OutboxSaga.Payment.Domain;

public record Money(decimal Amount, string Currency);

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed
}

public class Payment
{
    public string Id { get; private set; } = Guid.NewGuid().ToString("N");
    public string OrderId { get; private set; } = string.Empty;
    public Money Value { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Payment() { }

    public static Payment Create(string orderId, decimal amount, string currency)
    {
        return new Payment
        {
            OrderId = orderId,
            Value = new Money(amount, currency),
            Status = PaymentStatus.Pending
        };
    }

    public void Complete()
    {
        Status = PaymentStatus.Completed;
    }

    public void Fail()
    {
        Status = PaymentStatus.Failed;
    }
}
