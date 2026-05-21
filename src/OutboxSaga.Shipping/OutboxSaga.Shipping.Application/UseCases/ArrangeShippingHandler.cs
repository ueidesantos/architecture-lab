using OutboxSaga.Messaging;
using OutboxSaga.Shipping.Application.Abstractions;
using OutboxSaga.Shipping.Application.Messaging;

namespace OutboxSaga.Shipping.Application.UseCases;

public sealed class ArrangeShippingHandler
{
    private readonly IShippingRepository _shippingRepository;
    private readonly IInboxRepository _inboxRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArrangeShippingHandler(
        IShippingRepository shippingRepository,
        IInboxRepository inboxRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _shippingRepository = shippingRepository;
        _inboxRepository = inboxRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(PaymentProcessedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (!@event.Success) return;

        if (await _inboxRepository.HasBeenProcessedAsync(@event.Id, ct)) return;

        var shipping = Domain.Shipping.Create(@event.OrderId);
        shipping.Ship();

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCt =>
        {
            await _shippingRepository.AddAsync(shipping, transactionCt);

            var outcomeEvent = new ShippingArrangedIntegrationEvent
            {
                OrderId = @event.OrderId,
                ShippingId = shipping.Id,
                TrackingCode = shipping.TrackingCode,
                EstimatedDelivery = shipping.EstimatedDelivery,
                CorrelationId = @event.CorrelationId,
                CausationId = @event.Id
            };

            await _outboxRepository.AddAsync(OutboxMessage.FromIntegrationEvent(outcomeEvent), transactionCt);
            await _inboxRepository.MarkAsProcessedAsync(@event.Id, transactionCt);
        }, ct);
    }
}
