namespace OutboxSaga.Orders.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken ct = default);
}
