using Marten;
using Marten.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Oakton;
using OptimisticConcurrency.Core.Exceptions;
using OptimisticConcurrency.Immutable.ShoppingCarts;
using OptimisticConcurrency.Mixed.ShoppingCarts;
using OptimisticConcurrency.Mutable.ShoppingCarts;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRouting()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMutableShoppingCarts()
    .AddMixedShoppingCarts()
    .AddImmutableShoppingCarts()
    .AddMarten(options =>
    {
        var schemaName = Environment.GetEnvironmentVariable("SchemaName") ?? "Workshop_Optimistic_ShoppingCarts_Solved";
        options.Events.DatabaseSchemaName = schemaName;
        options.DatabaseSchemaName = schemaName;
        options.Connection(builder.Configuration.GetConnectionString("ShoppingCarts") ??
                           throw new InvalidOperationException());

        options.ConfigureImmutableShoppingCarts()
            .ConfigureMutableShoppingCarts()
            .ConfigureMixedShoppingCarts();
    })
    .ApplyAllDatabaseChangesOnStartup()
    .OptimizeArtifactWorkflow()
    .UseLightweightSessions();

builder.Host.ApplyOaktonExtensions();

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
                ConcurrencyException=> StatusCodes.Status412PreconditionFailed,
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

return await app.RunOaktonCommands(args);

namespace OptimisticConcurrency
{
    public partial class Program;
}
