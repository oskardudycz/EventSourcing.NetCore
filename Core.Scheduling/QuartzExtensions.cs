using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Core.Scheduling;

public static class QuartzExtensions
{
    public static IServiceCollection AddQuartzDefaults(
        this IServiceCollection services,
        TimeSpan? passageOfTimeInterval = null
    ) =>
        services
            .AddQuartz(q => q
                .AddPassageOfTime(passageOfTimeInterval ?? TimeSpan.FromMinutes(1))
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
        TimeSpan interval
    )
    {
        var jobKey = new JobKey($"PassageOfTimeJob_{interval}");
        q.AddJob<PassageOfTimeJob>(opts => opts.WithIdentity(jobKey));

        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity($"{jobKey}-trigger")
            .WithSimpleSchedule(x => x.WithInterval(interval))
        );

        return q;
    }
}
