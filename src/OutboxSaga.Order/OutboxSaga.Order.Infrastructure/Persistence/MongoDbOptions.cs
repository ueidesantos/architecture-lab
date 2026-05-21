namespace OutboxSaga.Orders.Infrastructure.Persistence;

public sealed class MongoDbOptions
{
    public string ConnectionString { get; init; } = string.Empty;
    public string DatabaseName { get; init; } = "OutboxSagaDb";
}
