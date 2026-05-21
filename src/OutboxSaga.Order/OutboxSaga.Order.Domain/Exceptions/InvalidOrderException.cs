namespace OutboxSaga.Orders.Domain.Exceptions;

public sealed class InvalidOrderException
    : DomainException
{
    public InvalidOrderException(string message)
        : base(message)
    {
    }
}
