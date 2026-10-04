using OutboxSaga.Infrastructure;
using OutboxSaga.Orders.Api.Endpoints;
using OutboxSaga.Orders.Application;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "OutboxSaga.Order API";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapOrdersEndpoints();

app.Run();
