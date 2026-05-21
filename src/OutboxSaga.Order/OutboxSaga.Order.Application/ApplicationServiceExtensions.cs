using Microsoft.Extensions.DependencyInjection;
using OutboxSaga.Orders.Application.Abstractions.UseCases;
using OutboxSaga.Orders.Application.Features.Orders.Create;

namespace OutboxSaga.Orders.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateOrderCommand, CreateOrderResult>, CreateOrderHandler>();

        return services;
    }
}
