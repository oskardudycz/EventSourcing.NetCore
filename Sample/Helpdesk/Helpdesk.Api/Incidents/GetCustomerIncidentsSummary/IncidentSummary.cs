using Marten;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;

namespace Helpdesk.Api.Incidents.GetCustomerIncidentsSummary;

public class CustomerIncidentsSummary
{
    public Guid Id { get; set; }
    public int Pending { get; set; }
    public int Resolved { get; set; }
    public int Acknowledged { get; set; }
    public int Closed { get; set; }
}

public class CustomerIncidentsSummaryProjection: MultiStreamProjection<CustomerIncidentsSummary, Guid>
{
    public CustomerIncidentsSummaryProjection()
    {
        Identity<IncidentLogged>(e => e.CustomerId);
        CustomGrouping(new CustomerIncidentsSummaryGrouper());
    }

    public void Apply(IncidentLogged logged, CustomerIncidentsSummary current)
    {
        current.Pending++;
    }

    public void Apply(IncidentResolved resolved, CustomerIncidentsSummary current)
    {
        current.Pending--;
        current.Resolved++;
    }

    public void Apply(ResolutionAcknowledgedByCustomer acknowledged, CustomerIncidentsSummary current)
    {
        current.Resolved--;
        current.Acknowledged++;
    }

    public void Apply(IncidentClosed closed, CustomerIncidentsSummary current)
    {
        current.Acknowledged--;
        current.Closed++;
    }
}

public class CustomerIncidentsSummaryGrouper: IAggregateGrouper<Guid>
{
    private readonly Type[] eventTypes =
    [
        typeof(IncidentResolved), typeof(ResolutionAcknowledgedByCustomer),
        typeof(IncidentClosed)
    ];

    public async Task Group(IQuerySession session, IEnumerable<IEvent> events, ITenantSliceGroup<Guid> grouping)
    {
        var filteredEvents = events
            .Where(ev => eventTypes.Contains(ev.EventType))
            .ToList();

        if (!filteredEvents.Any())
            return;

        var incidentIds = filteredEvents.Select(e => e.StreamId).ToList();

        var result = await session.Events.QueryRawEventDataOnly<IncidentLogged>()
            .Where(e => incidentIds.Contains(e.IncidentId))
            .Select(x => new { x.IncidentId, x.CustomerId })
            .ToListAsync();

        foreach (var group in result.Select(g =>
                     new { g.CustomerId, Events = filteredEvents.Where(ev => ev.StreamId == g.IncidentId) }))
        {
            grouping.AddEvents(group.CustomerId, group.Events);
        }
    }
}
