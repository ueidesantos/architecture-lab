namespace OutboxSaga.Orders.Infrastructure.Persistence;

public static class MongoCollectionNames
{
    public const string Orders = "orders";
    public const string OutboxMessages = "outbox_messages";
}
