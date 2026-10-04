namespace OutboxSaga.Orders.Application.Abstractions.UseCases;

public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default);
}
