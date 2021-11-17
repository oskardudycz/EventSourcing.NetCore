using Core.ElasticSearch;
using MeetingsSearch.Meetings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsSearch;

public static class Config
{
    public static void AddMeetingsSearch(this IServiceCollection services, IConfiguration config)
    {
        services.AddElasticsearch(config, settings =>
        {
            settings
                .DefaultMappingFor<Meeting>(m => m
                    .PropertyName(p => p.Id, "id")
                    .PropertyName(p => p.Name, "name")
                );
        });
        services.AddMeeting();
    }
}
