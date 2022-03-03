using Core.ElasticSearch.Repository;
using Core.Events;
using Core.Queries;
using MeetingsSearch.Meetings.CreatingMeeting;
using MeetingsSearch.Meetings.SearchingMeetings;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsSearch.Meetings;

public static class Config
{
    public static IServiceCollection AddMeeting(this IServiceCollection services) =>
        services
            .AddScoped<IElasticSearchRepository<Meeting>, ElasticSearchRepository<Meeting>>()
            .AddEventHandler<MeetingCreated, HandleMeetingCreated>()
            .AddQueryHandler<SearchMeetings, IReadOnlyCollection<Meeting>, HandleSearchMeetings>();
}
