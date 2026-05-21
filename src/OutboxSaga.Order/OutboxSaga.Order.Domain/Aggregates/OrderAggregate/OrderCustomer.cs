using OutboxSaga.Orders.Domain.Exceptions;

namespace OutboxSaga.Orders.Domain.Aggregates.OrderAggregate;

public sealed record OrderCustomer
{
    public OrderCustomer(string id, string name, string email)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOrderException("Customer id is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOrderException("Customer name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOrderException("Customer email is required.");
        }

        Id = id.Trim();
        Name = name.Trim();
        Email = email.Trim();
    }

    public string Id { get; }
    public string Name { get; }
    public string Email { get; }
}
