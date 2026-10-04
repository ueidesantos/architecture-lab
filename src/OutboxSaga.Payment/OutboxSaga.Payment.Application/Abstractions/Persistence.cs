namespace OutboxSaga.Payment.Application.Abstractions;

/// <summary>
/// Staff Architect Note:
/// The Inbox Pattern ensures that even if we receive the same message twice
/// (due to "At Least Once" delivery in Kafka), we only process it once.
/// </summary>
public interface IInboxRepository
{
    Task<bool> HasBeenProcessedAsync(string messageId, CancellationToken ct = default);
    Task MarkAsProcessedAsync(string messageId, CancellationToken ct = default);
}

public interface IPaymentRepository
{
    Task AddAsync(Domain.Payment payment, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default);
}
