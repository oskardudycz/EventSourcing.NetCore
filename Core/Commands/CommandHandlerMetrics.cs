using System.Diagnostics;
using System.Diagnostics.Metrics;
using Core.OpenTelemetry;

namespace Core.Commands;

public class CommandHandlerMetrics: IDisposable
{
    private readonly TimeProvider timeProvider;
    private readonly Meter meter;
    private readonly UpDownCounter<long> activeEventHandlingCounter;
    private readonly Counter<long> totalCommandsNumber;
    private readonly Histogram<double> eventHandlingDuration;

    public CommandHandlerMetrics(
        IMeterFactory meterFactory,
        TimeProvider timeProvider
    )
    {
        this.timeProvider = timeProvider;
        meter = meterFactory.Create(ActivitySourceProvider.DefaultSourceName);

        totalCommandsNumber = meter.CreateCounter<long>(
            TelemetryTags.Commands.TotalCommandsNumber,
            unit: "{command}",
            description: "Total number of commands send to command handlers");

        activeEventHandlingCounter = meter.CreateUpDownCounter<long>(
            TelemetryTags.Commands.ActiveCommandsNumber,
            unit: "{command}",
            description: "Number of commands currently being handled");

        eventHandlingDuration = meter.CreateHistogram<double>(
            TelemetryTags.Commands.CommandHandlingDuration,
            unit: "s",
            description: "Measures the duration of inbound commands");
    }

    public long CommandHandlingStart(string commandType)
    {
        var tags = new TagList { { TelemetryTags.Commands.CommandType, commandType } };

        if (activeEventHandlingCounter.Enabled)
        {
            activeEventHandlingCounter.Add(1, tags);
        }

        if (totalCommandsNumber.Enabled)
        {
            totalCommandsNumber.Add(1, tags);
        }

        return timeProvider.GetTimestamp();
    }

    public void CommandHandlingEnd(string commandType, long startingTimestamp)
    {
        var tags = activeEventHandlingCounter.Enabled
                   || eventHandlingDuration.Enabled
            ? new TagList { { TelemetryTags.Commands.CommandType, commandType } }
            : default;

        if (activeEventHandlingCounter.Enabled)
        {
            activeEventHandlingCounter.Add(-1, tags);
        }

        if (!eventHandlingDuration.Enabled) return;

        var elapsed = timeProvider.GetElapsedTime(startingTimestamp);

        eventHandlingDuration.Record(
            elapsed.TotalSeconds,
            tags);
    }

    public void Dispose() => meter.Dispose();
}
