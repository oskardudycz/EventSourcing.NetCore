using Helpdesk.Api.Incidents.GettingCustomerIncidents;
using Helpdesk.Api.Incidents.GettingCustomerIncidentsSummary;
using Helpdesk.Api.Incidents.GettingDetails;
using Helpdesk.Api.Incidents.GettingHistory;
using Helpdesk.Api.Incidents.ResolutionBatch;
using Helpdesk.Api.Incidents.Resolving;
using Marten;
using Marten.Events.Projections;
using Wolverine;

namespace Helpdesk.Api.Incidents;

public static class Configuration
{
    public static StoreOptions ConfigureIncidents(this StoreOptions options)
    {
        options.Projections.LiveStreamAggregation<Incident>();
        options.Projections.LiveStreamAggregation<IncidentsBatchResolution>();
        options.Projections.Add<IncidentHistoryTransformation>(ProjectionLifecycle.Inline);
        options.Projections.Add<IncidentDetailsProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<IncidentShortInfoProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<CustomerIncidentsSummaryProjection>(ProjectionLifecycle.Async);

        return options;
    }

    public static WolverineOptions ConfigureIncidents(this WolverineOptions options)
    {
        //Console.WriteLine(options.DescribeHandlerMatch(typeof(ResolveFromBatchHandler)));
        options.LocalQueue("incidents_batch_resolution")
            .Sequential();

        options.Publish(rule =>
        {
            rule.Message<InitiateIncidentsBatchResolution>();

            rule.Message<ResolveIncidentFromBatch>()
                .Message<IncidentResolved>()
                .Message<IncidentResolutionFailed>();

            rule.MessagesImplementing<IncidentsBatchResolutionEvent>();

            rule
                .ToLocalQueue("incidents_batch_resolution")
                .UseDurableInbox();
        });

        return options;
    }
}
