using Core.ElasticSearch;
using MeetingsSearch.Meetings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static Core.ElasticSearch.Indices.IndexNameMapper;

namespace MeetingsSearch;

public static class Config
{
    public static IServiceCollection AddMeetingsSearch(this IServiceCollection services, IConfiguration config) =>
        services
            .AddElasticsearch(config, settings =>
            {
                settings
                    .DefaultMappingFor<Meeting>(m => m
                        .IdProperty(p => p.Id)
                        .IndexName(ToIndexName<Meeting>())
                    );
            })
            .AddMeeting();
}
