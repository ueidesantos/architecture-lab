using OutboxSaga.Orders.Application.Abstractions.Messaging;
using OutboxSaga.Orders.Application.Abstractions.Persistence;
using OutboxSaga.Orders.Application.Abstractions.UseCases;
using OutboxSaga.Orders.Application.Messaging;
using OutboxSaga.Orders.Domain.Aggregates.OrderAggregate;

namespace OutboxSaga.Orders.Application.Features.Orders.Create;

public sealed class CreateOrderHandler
    : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateOrderResult> HandleAsync(CreateOrderCommand command, CancellationToken ct = default)
    {
        var customer = new OrderCustomer(
            command.CustomerId,
            command.CustomerName,
            command.CustomerEmail);

        var order = new Order(
            customer: customer,
            description: command.Description,
            totalValue: command.TotalValue
        );

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCt =>
        {
            await _orderRepository.AddAsync(order, transactionCt);

            foreach (var domainEvent in order.DomainEvents)
            {
                var outboxMessage = OutboxMessage.FromDomainEvent(
                    aggregateId: order.Id,
                    domainEvent: domainEvent);

                await _outboxRepository.AddAsync(outboxMessage, transactionCt);
            }

            order.ClearDomainEvents();
        }, ct);

        return new CreateOrderResult(order.Id);
    }
}
