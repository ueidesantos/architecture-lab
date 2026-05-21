using OutboxSaga.Messaging;
using OutboxSaga.Payment.Application.Abstractions;
using OutboxSaga.Payment.Application.Messaging;

namespace OutboxSaga.Payment.Application.UseCases;

/// <summary>
/// Staff Architect Note:
/// This handler demonstrates the "High Consistency" pattern:
/// 1. Inbox Check (Idempotency)
/// 2. Business Logic (Process Payment)
/// 3. Atomic Persistence (State + Outbox + Inbox mark)
/// </summary>
public sealed class ProcessPaymentHandler
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInboxRepository _inboxRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessPaymentHandler(
        IPaymentRepository paymentRepository,
        IInboxRepository inboxRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _inboxRepository = inboxRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(OrderCreatedIntegrationEvent @event, CancellationToken ct = default)
    {
        // 1. Inbox Check: Have we processed this specific message before?
        if (await _inboxRepository.HasBeenProcessedAsync(@event.Id, ct))
        {
            return; // Idempotent: Skip processing
        }

        // 2. Business Logic (Simulated)
        var payment = Domain.Payment.Create(@event.OrderId, @event.TotalValue, @event.Currency);

        // Simulate a successful payment
        payment.Complete();

        // 3. Atomic Persistence
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCt =>
        {
            // Save business state
            await _paymentRepository.AddAsync(payment, transactionCt);

            // Prepare outcome event
            var outcomeEvent = new PaymentProcessedIntegrationEvent
            {
                OrderId = @event.OrderId,
                Success = true,
                CorrelationId = @event.CorrelationId,
                CausationId = @event.Id
            };

            // Save to outbox
            var outboxMessage = OutboxMessage.FromIntegrationEvent(outcomeEvent);
            await _outboxRepository.AddAsync(outboxMessage, transactionCt);

            // Mark message as processed in Inbox
            await _inboxRepository.MarkAsProcessedAsync(@event.Id, transactionCt);

        }, ct);
    }
}
