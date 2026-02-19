namespace Core.OpenTelemetry;

public static class TelemetryPropagator
{
    private static TextMapPropagator propagator = Propagators.DefaultTextMapPropagator;

    public static void UseDefaultCompositeTextMapPropagator()
    {
        propagator =
            new CompositeTextMapPropagator([
                new TraceContextPropagator(), new BaggagePropagator()
            ]);
    }

    public static void Inject<T>(
        this PropagationContext context,
        T carrier,
        Action<T, string, string> setter
    ) =>
        propagator.Inject(context, carrier, setter);

    public static PropagationContext Extract<T>(
        T carrier,
        Func<T, string, IEnumerable<string>> getter
    ) =>
        propagator.Extract(default, carrier, getter);

    public static PropagationContext Extract<T>(
        PropagationContext context,
        T carrier,
        Func<T, string, IEnumerable<string>> getter
    ) =>
        propagator.Extract(context, carrier, getter);

    public static PropagationContext? Propagate<T>(this Activity? activity, T carrier, Action<T, string, string> setter)
    {
        if (activity?.Context == null) return null;

        var propagationContext = new PropagationContext(activity.Context, Baggage.Current);
        propagationContext.Inject(carrier, setter);

        return propagationContext;
    }

    public static PropagationContext? GetPropagationContext(Activity? activity = null)
    {
        var activityContext = (activity?? Activity.Current)?.Context;
        if (!activityContext.HasValue) return null;

        return new PropagationContext(activityContext.Value, Baggage.Current);
    }
}
