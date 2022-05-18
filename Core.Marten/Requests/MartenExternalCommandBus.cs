using Core.Requests;
using Core.Tracing;
using Marten;

namespace Core.Marten.Requests;

public class MartenExternalCommandBus: IExternalCommandBus
{
    private const string CommandsStreamName = "__commands";

    private readonly IDocumentSession documentSession;
    private readonly ITraceMetadataProvider traceMetadataProvider;

    public MartenExternalCommandBus(
        IDocumentSession documentSession,
        ITraceMetadataProvider traceMetadataProvider
    )
    {
        this.documentSession = documentSession;
        this.traceMetadataProvider = traceMetadataProvider;
    }

    public async Task Send<T>(T command, CancellationToken ct = default) where T: notnull
    {
        var traceMetadata = traceMetadataProvider.Get();

        documentSession.CorrelationId = traceMetadata?.CorrelationId?.Value;
        documentSession.CausationId = traceMetadata?.CausationId?.Value;

        documentSession.Events.Append(
            CommandsStreamName,
            command
        );

        await documentSession.SaveChangesAsync(ct);
    }
}
