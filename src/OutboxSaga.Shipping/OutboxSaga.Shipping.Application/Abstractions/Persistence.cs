namespace OutboxSaga.Shipping.Application.Abstractions;

public interface IInboxRepository
{
    Task<bool> HasBeenProcessedAsync(string messageId, CancellationToken ct = default);
    Task MarkAsProcessedAsync(string messageId, CancellationToken ct = default);
}

public interface IShippingRepository
{
    Task AddAsync(Domain.Shipping shipping, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default);
}
