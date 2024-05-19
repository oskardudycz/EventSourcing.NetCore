using ApplicationLogic.EventStoreDB.Core.Exceptions;
using ApplicationLogic.EventStoreDB.Immutable.ShoppingCarts;
using ApplicationLogic.EventStoreDB.Mixed.ShoppingCarts;
using ApplicationLogic.EventStoreDB.Mutable.ShoppingCarts;
using Core.Configuration;
using EventStore.Client;
using Microsoft.AspNetCore.Diagnostics;

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

public partial class Program;
