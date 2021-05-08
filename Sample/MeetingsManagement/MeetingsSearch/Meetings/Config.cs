using System.Collections.Generic;
using Core.Repositories;
using MediatR;
using MeetingsSearch.Meetings.Events;
using MeetingsSearch.Meetings.Queries;
using MeetingsSearch.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsSearch.Meetings
{
    public static class Config
    {
        public static void AddMeeting(this IServiceCollection services)
        {
            services.AddScoped<IRepository<Meeting>, ElasticSearchRepository<Meeting>>();
            services.AddScoped<INotificationHandler<MeetingCreated>, MeetingEventHandler>();
            services.AddScoped<IRequestHandler<SearchMeetings, IReadOnlyCollection<Meeting>>, MeetingQueryHandler>();
        }
    }
}
