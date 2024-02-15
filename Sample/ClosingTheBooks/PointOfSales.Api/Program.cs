using System.Text.Json.Serialization;
using Helpdesk.Api.Core.Http.Middlewares.ExceptionHandling;
using Helpdesk.Api.Core.Marten;
using JasperFx.CodeGeneration;
using Marten;
using Marten.AspNetCore;
using Marten.Events;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Marten.Exceptions;
using Marten.Schema.Identity;
using Marten.Services.Json;
using Oakton;
using PointOfSales.Api.Core;
using PointOfSales.CashierShifts;
using PointOfSales.CashRegister;
using Weasel.Core;
using static Microsoft.AspNetCore.Http.TypedResults;
using static PointOfSales.CashierShifts.CashierShiftDecider;
using static PointOfSales.CashRegister.CashRegisterDecider;
using static PointOfSales.CashierShifts.CashierShiftCommand;
using static PointOfSales.CashierShifts.CashierShiftEvent;
using static PointOfSales.Api.Core.ETagExtensions;
using static System.DateTimeOffset;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddDefaultExceptionHandler(
        (exception, _) => exception switch
        {
            ConcurrencyException =>
                exception.MapToProblemDetails(StatusCodes.Status412PreconditionFailed),
            ExistingStreamIdCollisionException =>
                exception.MapToProblemDetails(StatusCodes.Status412PreconditionFailed),
            _ => null,
        })
    .AddMarten(options =>
    {
        var schemaName = Environment.GetEnvironmentVariable("SchemaName") ?? "PointOfSales";
        options.Events.DatabaseSchemaName = schemaName;
        options.DatabaseSchemaName = schemaName;
        options.Connection(builder.Configuration.GetConnectionString("PointOfSales") ??
                           throw new InvalidOperationException());

        options.UseDefaultSerialization(
            EnumStorage.AsString,
            nonPublicMembersStorage: NonPublicMembersStorage.All,
            serializerType: SerializerType.SystemTextJson
        );

        // THIS IS IMPORTANT!
        options.Events.StreamIdentity = StreamIdentity.AsString;

        options.Projections.LiveStreamAggregation<CashierShift>();
        options.Projections.LiveStreamAggregation<CashRegister>();
        // Added as inline for now to make tests easier, should be async in the end
        options.Projections.Add<CashierShiftTrackerProjection>(ProjectionLifecycle.Inline);
    })
    .OptimizeArtifactWorkflow(TypeLoadMode.Static)
    .UseLightweightSessions()
    .AddAsyncDaemon(DaemonMode.Solo);

builder.Services
    .Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Host.ApplyOaktonExtensions();

var app = builder.Build();

app.UseExceptionHandler();

app.MapPost("/api/cash-registers/{cashRegisterId}",
    async (
        IDocumentSession documentSession,
        string cashRegisterId,
        CancellationToken ct) =>
    {
        await documentSession.Add<CashRegister>(cashRegisterId,
            Decide(new InitializeCashRegister(cashRegisterId, Now)), ct);

        return Created($"/api/cash-registers/{cashRegisterId}", cashRegisterId);
    }
);

app.MapPost("/api/cash-registers/{cashRegisterId}/cashier-shifts",
    async (
        IDocumentSession documentSession,
        string cashRegisterId,
        OpenShiftRequest body,
        CancellationToken ct
    ) =>
    {
        var lastClosedShift = await documentSession.GetLastCashierShift(cashRegisterId);
        var result = Decide(new OpenShift(cashRegisterId, body.CashierId, Now), lastClosedShift);

        if (result.Length == 0 || result[0] is not ShiftOpened opened)
            throw new InvalidOperationException("Cannot Open Shift");

        await documentSession.Add<CashierShift>(opened.CashierShiftId, result, ct);

        return Created(
            $"/api/cash-registers/{cashRegisterId}/cashier-shifts/{opened.CashierShiftId.ShiftNumber}",
            cashRegisterId
        );
    }
);

app.MapPost("/api/cash-registers/{cashRegisterId}/cashier-shifts/{shiftNumber:int}/transactions",
    (
        IDocumentSession documentSession,
        string cashRegisterId,
        int shiftNumber,
        RegisterTransactionRequest body,
        [FromIfMatchHeader] string eTag,
        CancellationToken ct
    ) =>
    {
        var cashierShiftId = new CashierShiftId(cashRegisterId, shiftNumber);
        var transactionId = CombGuidIdGeneration.NewGuid().ToString();

        return documentSession.GetAndUpdate<CashierShift>(cashierShiftId, ToExpectedVersion(eTag),
            state => Decide(new RegisterTransaction(cashierShiftId, transactionId, body.Amount, Now), state), ct);
    }
);

app.MapPost("/api/cash-registers/{cashRegisterId}/cashier-shifts/{shiftNumber:int}/close",
    (
        IDocumentSession documentSession,
        string cashRegisterId,
        int shiftNumber,
        CloseShiftRequest body,
        [FromIfMatchHeader] string eTag,
        CancellationToken ct
    ) =>
    {
        var cashierShiftId = new CashierShiftId(cashRegisterId, shiftNumber);

        return documentSession.GetAndUpdate<CashierShift>(cashierShiftId, ToExpectedVersion(eTag),
            state => Decide(new CloseShift(cashierShiftId, body.DeclaredTender, Now), state), ct);
    }
);

app.MapGet("/api/cash-registers/{cashRegisterId}/cashier-shifts/{shiftNumber:int}",
    (HttpContext context, IQuerySession querySession, string cashRegisterId, int shiftNumber) =>
        querySession.Json.WriteById<CashierShiftDetails>(new CashierShiftId(cashRegisterId, shiftNumber), context)
);


if (app.Environment.IsDevelopment())
{
    app.UseSwagger()
        .UseSwaggerUI();
}

return await app.RunOaktonCommands(args);

public record OpenShiftRequest(
    string CashierId
);

public record RegisterTransactionRequest(
    decimal Amount
);


public record CloseShiftRequest(
    decimal DeclaredTender
);


public partial class Program
{
}
