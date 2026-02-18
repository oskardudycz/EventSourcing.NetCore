using System.Text.Json.Serialization;
using Marten;
using Marten.Events.Aggregation;
using Marten.Schema.Identity;
using Weasel.Core;
using static Microsoft.AspNetCore.Http.Results;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMarten(options =>
    {
        var schemaName = Environment.GetEnvironmentVariable("SchemaName");
        if (!string.IsNullOrEmpty(schemaName))
        {
            options.Events.DatabaseSchemaName = schemaName;
            options.DatabaseSchemaName = schemaName;
        }

        options.AutoCreateSchemaObjects = AutoCreate.All;
        options.Connection(builder.Configuration.GetConnectionString("Incidents") ??
                           throw new InvalidOperationException());
        options.UseDefaultSerialization(EnumStorage.AsString, nonPublicMembersStorage: NonPublicMembersStorage.All);

        options.CreateDatabasesForTenants(c =>
        {
            c.ForTenant()
                .CheckAgainstPgDatabase()
                .WithOwner("postgres")
                .WithEncoding("UTF-8")
                .ConnectionLimit(-1);
        });

        options.Events.MetadataConfig.CausationIdEnabled = true;
        options.Events.MetadataConfig.CorrelationIdEnabled = true;

        options.Policies.ForAllDocuments(x =>
        {
            x.Metadata.CausationId.Enabled = true;
            x.Metadata.CorrelationId.Enabled = true;
        });

        options.Projections.Add<HotelChainProjection>();
    })
    .UseLightweightSessions()
    .OptimizeArtifactWorkflow();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o =>
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.MapPost("/api/chain", async (IDocumentSession session, string name, CancellationToken ct) =>
{
    var chainId = Guid.CreateVersion7();
    var @event = new HotelChainSetUp(chainId, name);

    var correlationId = Guid.NewGuid().ToString();
    session.CorrelationId = correlationId;
    session.CausationId = correlationId;
    session.Events.StartStream(chainId, @event);

    await session.SaveChangesAsync(ct);

    return Created($"/api/chain/{chainId}", chainId);
});

app.MapPost("/api/chain/{chainId:guid}/name",
    async (IDocumentSession session, Guid chainId, string name, CancellationToken ct) =>
    {
        var @event = new HotelChainNameChanged(chainId, name);

        var correlationId = Guid.NewGuid().ToString();
        session.CorrelationId = correlationId;
        session.CausationId = correlationId;
        session.Events.Append(chainId, @event);

        await session.SaveChangesAsync(ct);
    });

if (app.Environment.IsDevelopment())
{
    app.UseSwagger()
        .UseSwaggerUI();
}

app.Run();

public record HotelChainSetUp(
    Guid ChainId,
    string Name
);

public record HotelChainNameChanged(
    Guid ChainId,
    string Name
);

public record HotelChain(
    Guid Id,
    string Name
);

public class HotelChainProjection: SingleStreamAggregation<HotelChain>
{
    public static HotelChain Create(HotelChainSetUp @event) =>
        new(@event.ChainId, @event.Name);

    public HotelChain Apply(HotelChainNameChanged @event, HotelChain current) =>
        current with { Name = @event.Name };
}
