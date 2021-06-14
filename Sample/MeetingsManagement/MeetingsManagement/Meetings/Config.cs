using Core.Repositories;
using Core.Storage;
using Marten;
using MediatR;
using MeetingsManagement.Meetings.Commands;
using MeetingsManagement.Meetings.Projections;
using MeetingsManagement.Meetings.Queries;
using MeetingsManagement.Meetings.Views;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsManagement.Meetings
{
    public static class Config
    {
        public static void AddMeeting(this IServiceCollection services)
        {
            services.AddScoped<IRepository<Meeting>, MartenRepository<Meeting>>();

            services.AddScoped<IRequestHandler<CreateMeeting, Unit>, MeetingCommandHandler>();

            services.AddScoped<IRequestHandler<ScheduleMeeting, Unit>, MeetingCommandHandler>();

            services.AddScoped<IRequestHandler<GetMeeting, MeetingView?>, MeetingQueryHandler>();
        }

        public static void ConfigureMarten(StoreOptions options)
        {
            options.Projections.SelfAggregate<Meeting>();
            options.Projections.Add(new MeetingViewProjection());
        }
    }
}
