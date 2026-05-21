using OutboxSaga.Orders.Domain.Exceptions;

namespace OutboxSaga.Orders.Domain.ValueObjects;

public sealed record Money
{
    public Money(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
        {
            throw new InvalidOrderException("Money amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new InvalidOrderException("Money currency is required.");
        }

        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
    }

    public decimal Amount { get; }
    public string Currency { get; }

    public static Money Zero(string currency = "BRL") => new(0, currency);

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
        {
            throw new InvalidOrderException("Cannot add money with different currencies.");
        }

        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static implicit operator decimal(Money money)
        => money.Amount;
}
