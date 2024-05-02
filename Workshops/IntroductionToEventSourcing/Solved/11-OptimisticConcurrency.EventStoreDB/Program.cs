using Core.Configuration;
using EventStore.Client;
using Microsoft.AspNetCore.Diagnostics;
using OptimisticConcurrency.Core.Exceptions;
using OptimisticConcurrency.Immutable.ShoppingCarts;
using OptimisticConcurrency.Mixed.ShoppingCarts;
using OptimisticConcurrency.Mutable.ShoppingCarts;

var builder = WebApplication.CreateBuilder(args);

var eventStoreClient = new EventStoreClient(
    EventStoreClientSettings.Create(builder.Configuration.GetRequiredConnectionString(("ShoppingCarts")))
);

builder.Services
    .AddRouting()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMutableShoppingCarts()
    .AddMixedShoppingCarts()
    .AddImmutableShoppingCarts()
    .AddSingleton(eventStoreClient);

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions
    {
        AllowStatusCode404Response = true,
        ExceptionHandler = context =>
        {
            var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

            Console.WriteLine("ERROR: " + exception);

            context.Response.StatusCode = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                InvalidOperationException => StatusCodes.Status409Conflict,
                BadHttpRequestException
                {
                    Message: "Required parameter \"string eTag\" was not provided from header."
                } => StatusCodes.Status412PreconditionFailed,
                WrongExpectedVersionException => StatusCodes.Status412PreconditionFailed,
                _ => StatusCodes.Status500InternalServerError,
            };

            return Task.CompletedTask;
        }
    })
    .UseRouting();

app.ConfigureImmutableShoppingCarts()
    .ConfigureMutableShoppingCarts()
    .ConfigureMixedShoppingCarts();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger()
        .UseSwaggerUI();
}

await app.RunAsync();

namespace OptimisticConcurrency
{
    public partial class Program
    {
    }
}
