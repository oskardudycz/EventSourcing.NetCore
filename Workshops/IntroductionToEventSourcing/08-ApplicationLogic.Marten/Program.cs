using ApplicationLogic.Marten.Core.Exceptions;
using ApplicationLogic.Marten.Immutable.ShoppingCarts;
using ApplicationLogic.Marten.Mixed.ShoppingCarts;
using ApplicationLogic.Marten.Mutable.ShoppingCarts;
using JasperFx;
using JasperFx.CodeGeneration;
using Marten;
using Microsoft.AspNetCore.Diagnostics;
using Oakton;

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
        var schemaName = Environment.GetEnvironmentVariable("SchemaName") ?? "Workshop_Application_ShoppingCarts";
        options.Events.DatabaseSchemaName = schemaName;
        options.DatabaseSchemaName = schemaName;
        options.Connection(builder.Configuration.GetConnectionString("ShoppingCarts") ??
                           throw new InvalidOperationException());

        options.ConfigureImmutableShoppingCarts()
            .ConfigureMutableShoppingCarts()
            .ConfigureMixedShoppingCarts();
    })
    .UseLightweightSessions();

// Configure production vs development settings
builder.Services.CritterStackDefaults(x =>
{
    x.Production.GeneratedCodeMode = TypeLoadMode.Static;
    x.Production.ResourceAutoCreate = AutoCreate.None;
});

builder.Host.ApplyJasperFxExtensions();

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

return await app.RunJasperFxCommands(args);

public partial class Program;
