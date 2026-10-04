using OutboxSaga.Orders.Application.Messaging;

namespace OutboxSaga.Orders.Application.Abstractions.Messaging;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken ct = default);
    Task<IReadOnlyList<OutboxMessage>> GetUnpublishedAsync(int batchSize, CancellationToken ct = default);
    Task MarkAsPublishedAsync(string messageId, DateTime publishedAtUtc, CancellationToken ct = default);
}
