using Core.Tracing.Causation;
using Core.Tracing.Correlation;
using Microsoft.AspNetCore.Http;

namespace Core.WebApi.Tracing;

public static class TracingHeadersExtensions
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string CausationIdHeaderName = "X-Causation-ID";

    public static CorrelationId? GetCorrelationId(this HttpRequest request) =>
        request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId)
            ? new CorrelationId(correlationId)
            : null;


    public static CausationId? GetCausationId(this HttpRequest request) =>
        request.Headers.TryGetValue(CausationIdHeaderName, out var correlationId)
            ? new CausationId(correlationId)
            : null;

    public static void SetCorrelationId(this HttpResponse response, CorrelationId correlationId) =>
        response.Headers.Add(CorrelationIdHeaderName, new[] { correlationId.Value });


    public static void SetCausationId(this HttpResponse response, CausationId causationId) =>
        response.Headers.Add(CausationIdHeaderName, new[] { causationId.Value });


}
