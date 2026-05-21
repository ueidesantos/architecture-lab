using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using OutboxSaga.Orders.Application.Abstractions.Messaging;
using OutboxSaga.Orders.Application.Abstractions.Persistence;
using OutboxSaga.Orders.Infrastructure.Persistence;
using OutboxSaga.Orders.Infrastructure.Persistence.Repositories;

namespace OutboxSaga.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        MongoMappings.Configure();

        var mongoOptions = configuration
            .GetSection("MongoDb")
            .Get<MongoDbOptions>()
            ?? throw new InvalidOperationException("MongoDb configuration is required.");

        if (string.IsNullOrWhiteSpace(mongoOptions.ConnectionString))
        {
            throw new InvalidOperationException("MongoDb:ConnectionString is required.");
        }

        if (string.IsNullOrWhiteSpace(mongoOptions.DatabaseName))
        {
            throw new InvalidOperationException("MongoDb:DatabaseName is required.");
        }

        services.AddSingleton(mongoOptions);
        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoOptions.ConnectionString));
        services.AddScoped<MongoContext>();
        services.AddScoped<IUnitOfWork, MongoUnitOfWork>();
        services.AddScoped<IOrderRepository, MongoOrderRepository>();
        services.AddScoped<IOutboxRepository, MongoOutboxRepository>();

        return services;
    }
}
