using OutboxSaga.Orders.Domain.Aggregates.OrderAggregate;

namespace OutboxSaga.Orders.Application.Abstractions.Persistence;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken ct = default);
    Task<Order?> GetByIdAsync(string id, string customerId, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default);
}
