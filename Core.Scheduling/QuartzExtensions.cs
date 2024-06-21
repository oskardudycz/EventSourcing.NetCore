using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Core.Scheduling;

public static class QuartzExtensions
{
    public static IServiceCollection AddQuartzDefaults(this IServiceCollection services) =>
        services
            .AddQuartz(q => q
                    .AddPassageOfTime(TimeUnit.Minute)
                    .AddPassageOfTime(TimeUnit.Hour)
                    .AddPassageOfTime(TimeUnit.Day)
                // .UsePersistentStore(x =>
                // {
                //     x.UseProperties = false;
                //     x.PerformSchemaValidation = false;
                //     x.UsePostgres(postgresConnectionString);
                // })
            )
            .AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

    public static IServiceCollectionQuartzConfigurator AddPassageOfTime(
        this IServiceCollectionQuartzConfigurator q,
        TimeUnit timeUnit
    )
    {
        var jobKey = new JobKey($"PassageOfTimeJob_{timeUnit}");
        q.AddJob<PassageOfTimeJob>(opts => opts.WithIdentity(jobKey));

        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity($"{jobKey}-trigger")
            .UsingJobData("timeUnit", timeUnit.ToString())
            .WithSimpleSchedule(x => x.WithInterval(timeUnit.ToTimeSpan()))
        );

        return q;
    }
}
