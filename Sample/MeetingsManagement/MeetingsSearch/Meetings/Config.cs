using System.Collections.Generic;
using Core.ElasticSearch.Repository;
using Core.Repositories;
using MediatR;
using MeetingsSearch.Meetings.CreatingMeeting;
using MeetingsSearch.Meetings.SearchingMeetings;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsSearch.Meetings
{
    public static class Config
    {
        public static void AddMeeting(this IServiceCollection services)
        {
            services.AddScoped<IRepository<Meeting>, ElasticSearchRepository<Meeting>>();
            services.AddScoped<INotificationHandler<MeetingCreated>, HandleMeetingCreated>();
            services.AddScoped<IRequestHandler<SearchMeetings, IReadOnlyCollection<Meeting>>, HandleSearchMeetings>();
        }
    }
}
