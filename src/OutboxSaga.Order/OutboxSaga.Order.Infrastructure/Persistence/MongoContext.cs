using MongoDB.Driver;
using OutboxSaga.Orders.Application.Messaging;
using OutboxSaga.Orders.Domain.Aggregates.OrderAggregate;

namespace OutboxSaga.Orders.Infrastructure.Persistence;

public sealed class MongoContext
{
    private readonly IMongoDatabase _database;

    public MongoContext(IMongoClient mongoClient, MongoDbOptions options)
    {
        _database = mongoClient.GetDatabase(options.DatabaseName);
    }

    public IClientSessionHandle? Session { get; private set; }

    public IMongoCollection<Order> Orders
        => _database.GetCollection<Order>(MongoCollectionNames.Orders);

    public IMongoCollection<OutboxMessage> OutboxMessages
        => _database.GetCollection<OutboxMessage>(MongoCollectionNames.OutboxMessages);

    public void UseSession(IClientSessionHandle session)
    {
        Session = session;
    }

    public void ClearSession()
    {
        Session = null;
    }
}
