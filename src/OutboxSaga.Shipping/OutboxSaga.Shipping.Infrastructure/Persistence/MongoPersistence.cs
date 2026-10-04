using MongoDB.Driver;
using OutboxSaga.Shipping.Application.Abstractions;
using OutboxSaga.Shipping.Application.Messaging;

namespace OutboxSaga.Shipping.Infrastructure.Persistence;

public sealed class MongoShippingContext
{
    private readonly IMongoDatabase _database;
    public MongoShippingContext(IMongoClient client, string databaseName)
    {
        _database = client.GetDatabase(databaseName);
    }

    public IClientSessionHandle? Session { get; private set; }

    public IMongoCollection<Domain.Shipping> Shippings => _database.GetCollection<Domain.Shipping>("shippings");
    public IMongoCollection<OutboxMessage> OutboxMessages => _database.GetCollection<OutboxMessage>("outbox");
    public IMongoCollection<InboxEntry> Inbox => _database.GetCollection<InboxEntry>("inbox");

    public async Task<IClientSessionHandle> StartSessionAsync(CancellationToken ct)
    {
        Session = await _database.Client.StartSessionAsync(cancellationToken: ct);
        return Session;
    }
}

public record InboxEntry(string MessageId, DateTime ProcessedAtUtc);

public class MongoPersistence : IShippingRepository, IInboxRepository, IOutboxRepository, IUnitOfWork
{
    private readonly MongoShippingContext _context;

    public MongoPersistence(MongoShippingContext context) => _context = context;

    public async Task AddAsync(Domain.Shipping shipping, CancellationToken ct)
        => await _context.Shippings.InsertOneAsync(_context.Session, shipping, cancellationToken: ct);

    public async Task<bool> HasBeenProcessedAsync(string messageId, CancellationToken ct)
    {
        var filter = Builders<InboxEntry>.Filter.Eq(x => x.MessageId, messageId);
        return await _context.Inbox.Find(filter).AnyAsync(ct);
    }

    public async Task MarkAsProcessedAsync(string messageId, CancellationToken ct)
        => await _context.Inbox.InsertOneAsync(_context.Session, new InboxEntry(messageId, DateTime.UtcNow), cancellationToken: ct);

    public async Task AddAsync(OutboxMessage message, CancellationToken ct)
        => await _context.OutboxMessages.InsertOneAsync(_context.Session, message, cancellationToken: ct);

    public async Task<IReadOnlyList<OutboxMessage>> GetUnpublishedAsync(int batchSize, CancellationToken ct)
    {
        var filter = Builders<OutboxMessage>.Filter.Eq(x => x.PublishedAtUtc, null);
        return await _context.OutboxMessages.Find(filter).Limit(batchSize).ToListAsync(ct);
    }

    public async Task MarkAsPublishedAsync(string messageId, DateTime publishedAtUtc, CancellationToken ct)
    {
        var filter = Builders<OutboxMessage>.Filter.Eq(x => x.Id, messageId);
        var update = Builders<OutboxMessage>.Update.Set(x => x.PublishedAtUtc, publishedAtUtc);
        await _context.OutboxMessages.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct)
    {
        using var session = await _context.StartSessionAsync(ct);
        session.StartTransaction();
        try
        {
            await action(ct);
            await session.CommitTransactionAsync(ct);
        }
        catch
        {
            await session.AbortTransactionAsync(ct);
            throw;
        }
    }
}
