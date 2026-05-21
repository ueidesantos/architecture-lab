using MongoDB.Driver;
using OutboxSaga.Orders.Application.Abstractions.Messaging;
using OutboxSaga.Orders.Application.Messaging;

namespace OutboxSaga.Orders.Infrastructure.Persistence.Repositories;

public sealed class MongoOutboxRepository : IOutboxRepository
{
    private readonly MongoContext _context;

    public MongoOutboxRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken ct = default)
    {
        if (_context.Session is not null)
        {
            await _context.OutboxMessages.InsertOneAsync(_context.Session, message, cancellationToken: ct);
            return;
        }

        await _context.OutboxMessages.InsertOneAsync(message, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetUnpublishedAsync(
        int batchSize,
        CancellationToken ct = default)
    {
        var filter = Builders<OutboxMessage>.Filter.Eq(message => message.PublishedAtUtc, null);
        var sort = Builders<OutboxMessage>.Sort.Ascending(message => message.CreatedAtUtc);

        return await _context.OutboxMessages
            .Find(filter)
            .Sort(sort)
            .Limit(batchSize)
            .ToListAsync(ct);
    }

    public async Task MarkAsPublishedAsync(
        string messageId,
        DateTime publishedAtUtc,
        CancellationToken ct = default)
    {
        var filter = Builders<OutboxMessage>.Filter.Eq(message => message.Id, messageId);
        var update = Builders<OutboxMessage>.Update.Set(message => message.PublishedAtUtc, publishedAtUtc);

        if (_context.Session is not null)
        {
            await _context.OutboxMessages.UpdateOneAsync(_context.Session, filter, update, cancellationToken: ct);
            return;
        }

        await _context.OutboxMessages.UpdateOneAsync(filter, update, cancellationToken: ct);
    }
}
