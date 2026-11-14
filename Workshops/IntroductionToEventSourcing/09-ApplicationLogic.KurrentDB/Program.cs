using ApplicationLogic.KurrentDB.Core.Exceptions;
using ApplicationLogic.KurrentDB.Immutable.ShoppingCarts;
using ApplicationLogic.KurrentDB.Mixed.ShoppingCarts;
using ApplicationLogic.KurrentDB.Mutable.ShoppingCarts;
using Core.Configuration;
using KurrentDB.Client;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var kurrentDBClient = new KurrentDBClient(
    KurrentDBClientSettings.Create(builder.Configuration.GetRequiredConnectionString(("ShoppingCarts")))
);

builder.Services
    .AddRouting()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMutableShoppingCarts()
    .AddMixedShoppingCarts()
    .AddImmutableShoppingCarts()
    .AddSingleton(kurrentDBClient);

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
