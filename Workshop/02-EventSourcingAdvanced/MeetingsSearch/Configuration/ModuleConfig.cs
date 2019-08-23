using MeetingsSearch.Meetings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsSearch.Configuration
{
    public static class ModuleConfig
    {
        public static void AddMeetingsSearch(this IServiceCollection services, IConfiguration config)
        {
            services.AddElasticsearch(config);
            services.AddMeeting();
        }
    }
}
