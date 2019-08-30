using Core.Storage;
using Marten;
using MediatR;
using MeetingsManagement.Meetings.Commands;
using MeetingsManagement.Meetings.Queries;
using MeetingsManagement.Meetings.ValueObjects;
using MeetingsManagement.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsManagement.Meetings
{
    public static class Config
    {
        public static void AddMeeting(this IServiceCollection services)
        {
            services.AddScoped<IRepository<Meeting>, MartenRepository<Meeting>>();

            services.AddScoped<IRequestHandler<CreateMeeting, Unit>, MeetingCommandHandler>();

            services.AddScoped<IRequestHandler<GetMeeting, MeetingSummary>, MeetingQueryHandler>();
        }

        public static void ConfigureMarten(StoreOptions options)
        {
            options.Events.InlineProjections.AggregateStreamsWith<Meeting>();
        }
    }
}
