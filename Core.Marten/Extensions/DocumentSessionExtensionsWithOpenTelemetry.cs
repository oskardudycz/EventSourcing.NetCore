using System.Runtime.CompilerServices;
using Core.Marten.Extensions;
using Core.OpenTelemetry;
using Core.Structures;
using Marten;

namespace Core.Marten.OpenTelemetry;

public static class DocumentSessionExtensionsWithOpenTelemetry
{
    public static Task Add<T>(
        this IDocumentSession documentSession,
        Guid id,
        object @event,
        CancellationToken ct
    ) where T : class =>
        documentSession.WithTelemetry<T>(
            token => DocumentSessionExtensions.Add<T>(documentSession, id, @event, token),
            ct
        );

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        int version,
        Func<T, object> handle,
        Func<T> initial,
        CancellationToken ct
    ) where T : class =>
        documentSession.WithTelemetry<T>(
            token => DocumentSessionExtensions.GetAndUpdate(documentSession, id, version, handle, initial, token),
            ct
        );

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        Func<T, object> handle,
        Func<T> initial,
        CancellationToken ct
    ) where T : class =>
        documentSession.WithTelemetry<T>(
            token => DocumentSessionExtensions.GetAndUpdate(documentSession, id, handle, initial, token),
            ct
        );

    public static Task GetAndUpdate<T, TEvent>(
        this IDocumentSession documentSession,
        Guid id,
        Func<T, Maybe<TEvent>> handle,
        Func<T> initial,
        CancellationToken ct
    ) where T : class =>
        documentSession.WithTelemetry<T>(
            token => DocumentSessionExtensions.GetAndUpdate(documentSession, id, handle, initial, token),
            ct
        );

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        Func<T, Maybe<object>> handle,
        Func<T> initial,
        CancellationToken ct
    ) where T : class =>
        documentSession.WithTelemetry<T>(
            token => DocumentSessionExtensions.GetAndUpdate(documentSession, id, handle, initial, token),
            ct
        );

    public static Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        Guid id,
        Func<T, Maybe<object[]>> handle,
        Func<T> initial,
        CancellationToken ct
    ) where T : class =>
        documentSession.WithTelemetry<T>(
            token => DocumentSessionExtensions.GetAndUpdate(documentSession, id, handle, initial, token),
            ct
        );

    private static Task WithTelemetry<T>(
        this IDocumentSession documentSession,
        Func<CancellationToken, Task> run,
        CancellationToken ct,
        [CallerMemberName] string memberName = ""
    ) =>
        ActivityScope.Instance.Run($"{nameof(DocumentSessionExtensions)}/{memberName}",
            (activity, token) =>
            {
                documentSession.PropagateTelemetry(activity);

                return run(token);
            },
            new StartActivityOptions { Tags = { { TelemetryTags.Logic.Stream, typeof(T).Name } } },
            ct);
}
