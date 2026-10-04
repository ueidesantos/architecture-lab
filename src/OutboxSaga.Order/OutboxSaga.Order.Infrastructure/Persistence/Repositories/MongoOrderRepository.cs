using MongoDB.Driver;
using OutboxSaga.Orders.Application.Abstractions.Persistence;
using OutboxSaga.Orders.Domain.Aggregates.OrderAggregate;
using OutboxSaga.Orders.Infrastructure.Persistence;

namespace OutboxSaga.Orders.Infrastructure.Persistence.Repositories;

public sealed class MongoOrderRepository : IOrderRepository
{
    private readonly MongoContext _context;

    public MongoOrderRepository(MongoContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        if (_context.Session is not null)
        {
            await _context.Orders.InsertOneAsync(_context.Session, order, cancellationToken: ct);
            return;
        }

        await _context.Orders.InsertOneAsync(order, cancellationToken: ct);
    }

    public async Task<Order?> GetByIdAsync(string id, string customerId, CancellationToken ct = default)
    {
        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.Eq(order => order.Id, id),
            Builders<Order>.Filter.Eq(order => order.Customer.Id, customerId));

        return await _context.Orders.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Orders.Find(Builders<Order>.Filter.Empty).ToListAsync(ct);
    }
}
