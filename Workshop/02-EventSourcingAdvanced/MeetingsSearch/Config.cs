using MeetingsSearch.Meetings;
using MeetingsSearch.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsSearch
{
    public static class Config
    {
        public static void AddMeetingsSearch(this IServiceCollection services, IConfiguration config)
        {
            services.AddElasticsearch(config);
            services.AddMeeting();
        }
    }
}
