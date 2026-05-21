using MongoDB.Driver;
using OutboxSaga.Orders.Application.Abstractions.Persistence;

namespace OutboxSaga.Orders.Infrastructure.Persistence;

public sealed class MongoUnitOfWork : IUnitOfWork
{
    private readonly IMongoClient _mongoClient;
    private readonly MongoContext _context;

    public MongoUnitOfWork(IMongoClient mongoClient, MongoContext context)
    {
        _mongoClient = mongoClient;
        _context = context;
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken ct = default)
    {
        using var session = await _mongoClient.StartSessionAsync(cancellationToken: ct);

        _context.UseSession(session);
        session.StartTransaction();

        try
        {
            await operation(ct);
            await session.CommitTransactionAsync(ct);
        }
        catch
        {
            await session.AbortTransactionAsync(ct);
            throw;
        }
        finally
        {
            _context.ClearSession();
        }
    }
}
