using MongoDB.Bson.Serialization;
using OutboxSaga.Orders.Domain.Aggregates.OrderAggregate;
using OutboxSaga.Orders.Domain.ValueObjects;

namespace OutboxSaga.Orders.Infrastructure.Persistence;

public static class MongoMappings
{
    private static bool _configured;

    public static void Configure()
    {
        if (_configured)
        {
            return;
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(OrderCustomer)))
        {
            BsonClassMap.RegisterClassMap<OrderCustomer>(map =>
            {
                map.MapCreator(customer => new OrderCustomer(customer.Id, customer.Name, customer.Email));
                map.MapMember(customer => customer.Id);
                map.MapMember(customer => customer.Name);
                map.MapMember(customer => customer.Email);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Money)))
        {
            BsonClassMap.RegisterClassMap<Money>(map =>
            {
                map.MapCreator(money => new Money(money.Amount, money.Currency));
                map.MapMember(money => money.Amount);
                map.MapMember(money => money.Currency);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(OrderStatus)))
        {
            BsonClassMap.RegisterClassMap<OrderStatus>(map =>
            {
                map.MapCreator(status => OrderStatus.From(status.Value));
                map.MapMember(status => status.Value);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Order)))
        {
            BsonClassMap.RegisterClassMap<Order>(map =>
            {
                map.MapCreator(order => Order.Rehydrate(
                    order.Id,
                    order.Customer,
                    order.Description,
                    order.TotalValue,
                    order.Status,
                    order.CreatedAt));

                map.MapIdMember(order => order.Id);
                map.MapMember(order => order.Customer);
                map.MapMember(order => order.Description);
                map.MapMember(order => order.TotalValue);
                map.MapMember(order => order.Status);
                map.MapMember(order => order.CreatedAt);
            });
        }

        _configured = true;
    }
}
