using MongoDB.Driver;
using OutboxSaga.Payment.Application.Abstractions;
using OutboxSaga.Payment.Application.Messaging;

namespace OutboxSaga.Payment.Infrastructure.Persistence;

public sealed class MongoPaymentContext
{
    private readonly IMongoDatabase _database;
    public MongoPaymentContext(IMongoClient client, string databaseName)
    {
        _database = client.GetDatabase(databaseName);
    }

    public IClientSessionHandle? Session { get; private set; }

    public IMongoCollection<Domain.Payment> Payments => _database.GetCollection<Domain.Payment>("payments");
    public IMongoCollection<OutboxMessage> OutboxMessages => _database.GetCollection<OutboxMessage>("outbox");
    public IMongoCollection<InboxEntry> Inbox => _database.GetCollection<InboxEntry>("inbox");

    public async Task<IClientSessionHandle> StartSessionAsync(CancellationToken ct)
    {
        Session = await _database.Client.StartSessionAsync(cancellationToken: ct);
        return Session;
    }
}

public record InboxEntry(string MessageId, DateTime ProcessedAtUtc);

public class MongoPersistence : IPaymentRepository, IInboxRepository, IOutboxRepository, IUnitOfWork
{
    private readonly MongoPaymentContext _context;

    public MongoPersistence(MongoPaymentContext context) => _context = context;

    // IPaymentRepository
    public async Task AddAsync(Domain.Payment payment, CancellationToken ct)
        => await _context.Payments.InsertOneAsync(_context.Session, payment, cancellationToken: ct);

    // IInboxRepository
    public async Task<bool> HasBeenProcessedAsync(string messageId, CancellationToken ct)
    {
        var filter = Builders<InboxEntry>.Filter.Eq(x => x.MessageId, messageId);
        return await _context.Inbox.Find(filter).AnyAsync(ct);
    }

    public async Task MarkAsProcessedAsync(string messageId, CancellationToken ct)
        => await _context.Inbox.InsertOneAsync(_context.Session, new InboxEntry(messageId, DateTime.UtcNow), cancellationToken: ct);

    // IOutboxRepository
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

    // IUnitOfWork
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
