// Taken from: https://github.com/davidfowl/WaitForDependenciesAspire/blob/main/WaitForDependencies.Aspire.Hosting/WaitForDependenciesExtensions.cs
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Aspire.Hosting;

public static class WaitForDependenciesExtensions
{
    /// <summary>
    /// Wait for a resource to be running before starting another resource.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="other">The resource to wait for.</param>
    public static IResourceBuilder<T> WaitFor<T>(this IResourceBuilder<T> builder, IResourceBuilder<IResource> other)
        where T : IResource
    {
        builder.ApplicationBuilder.AddWaitForDependencies();
        return builder.WithAnnotation(new WaitOnAnnotation(other.Resource));
    }

    /// <summary>
    /// Wait for a resource to run to completion before starting another resource.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="other">The resource to wait for.</param>
    public static IResourceBuilder<T> WaitForCompletion<T>(this IResourceBuilder<T> builder, IResourceBuilder<IResource> other)
        where T : IResource
    {
        builder.ApplicationBuilder.AddWaitForDependencies();
        return builder.WithAnnotation(new WaitOnAnnotation(other.Resource) { WaitUntilCompleted = true });
    }

    /// <summary>
    /// Adds a lifecycle hook that waits for all dependencies to be "running" before starting resources. If that resource
    /// has a health check, it will be executed before the resource is considered "running".
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    private static IDistributedApplicationBuilder AddWaitForDependencies(this IDistributedApplicationBuilder builder)
    {
        builder.Services.TryAddLifecycleHook<WaitForDependenciesRunningHook>();
        return builder;
    }

    private class WaitOnAnnotation(IResource resource) : IResourceAnnotation
    {
        public IResource Resource { get; } = resource;

        public string[]? States { get; set; }

        public bool WaitUntilCompleted { get; set; }
    }

    private class WaitForDependenciesRunningHook(DistributedApplicationExecutionContext executionContext,
        ResourceNotificationService resourceNotificationService) :
        IDistributedApplicationLifecycleHook,
        IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts = new();

        public Task BeforeStartAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
        {
            // We don't need to execute any of this logic in publish mode
            if (executionContext.IsPublishMode)
            {
                return Task.CompletedTask;
            }

            // The global list of resources being waited on
            var waitingResources = new ConcurrentDictionary<IResource, ConcurrentDictionary<WaitOnAnnotation, TaskCompletionSource>>();

            // For each resource, add an environment callback that waits for dependencies to be running
            foreach (var r in appModel.Resources)
            {
                var resourcesToWaitOn = r.Annotations.OfType<WaitOnAnnotation>().ToLookup(a => a.Resource);

                if (resourcesToWaitOn.Count == 0)
                {
                    continue;
                }

                // Abuse the environment callback to wait for dependencies to be running

                r.Annotations.Add(new EnvironmentCallbackAnnotation(async context =>
                {
                    var dependencies = new List<Task>();

                    // Find connection strings and endpoint references and get the resource they point to
                    foreach (var group in resourcesToWaitOn)
                    {
                        var resource = group.Key;

                        // REVIEW: This logic does not handle cycles in the dependency graph (that would result in a deadlock)

                        // Don't wait for yourself
                        if (resource != r && resource is not null)
                        {
                            var pendingAnnotations = waitingResources.GetOrAdd(resource, _ => new());

                            foreach (var waitOn in group)
                            {
                                var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

                                async Task Wait()
                                {
                                    context.Logger?.LogInformation("Waiting for {Resource}.", waitOn.Resource.Name);

                                    await tcs.Task;

                                    context.Logger?.LogInformation("Waiting for {Resource} completed.", waitOn.Resource.Name);
                                }

                                pendingAnnotations[waitOn] = tcs;

                                dependencies.Add(Wait());
                            }
                        }
                    }

                    await resourceNotificationService.PublishUpdateAsync(r, s => s with
                    {
                        State = new("Waiting", KnownResourceStateStyles.Info)
                    });

                    await Task.WhenAll(dependencies).WaitAsync(context.CancellationToken);
                }));
            }

            _ = Task.Run(async () =>
           {
               var stoppingToken = _cts.Token;

               // These states are terminal but we need a better way to detect that
               static bool IsKnownTerminalState(CustomResourceSnapshot snapshot) =>
                   snapshot.State == "FailedToStart" ||
                   snapshot.State == "Exited" ||
                   snapshot.ExitCode is not null;

               // Watch for global resource state changes
               await foreach (var resourceEvent in resourceNotificationService.WatchAsync(stoppingToken))
               {
                   if (waitingResources.TryGetValue(resourceEvent.Resource, out var pendingAnnotations))
                   {
                       foreach (var (waitOn, tcs) in pendingAnnotations)
                       {
                           if (waitOn.States is string[] states && states.Contains(resourceEvent.Snapshot.State?.Text, StringComparer.Ordinal))
                           {
                               pendingAnnotations.TryRemove(waitOn, out _);

                               _ = DoTheHealthCheck(resourceEvent, tcs);
                           }
                           else if (waitOn.WaitUntilCompleted)
                           {
                               if (IsKnownTerminalState(resourceEvent.Snapshot))
                               {
                                   pendingAnnotations.TryRemove(waitOn, out _);

                                   _ = DoTheHealthCheck(resourceEvent, tcs);
                               }
                           }
                           else if (waitOn.States is null)
                           {
                               if (resourceEvent.Snapshot.State == "Running")
                               {
                                   pendingAnnotations.TryRemove(waitOn, out _);

                                   _ = DoTheHealthCheck(resourceEvent, tcs);
                               }
                               else if (IsKnownTerminalState(resourceEvent.Snapshot))
                               {
                                   pendingAnnotations.TryRemove(waitOn, out _);

                                   tcs.TrySetException(new Exception($"Dependency {waitOn.Resource.Name} failed to start"));
                               }
                           }
                       }
                   }
               }
           },
           cancellationToken);

            return Task.CompletedTask;
        }

        private async Task DoTheHealthCheck(ResourceEvent resourceEvent, TaskCompletionSource tcs)
        {
            var resource = resourceEvent.Resource;

            // REVIEW: Right now, every resource does an independent health check, we could instead cache
            // the health check result and reuse it for all resources that depend on the same resource


            HealthCheckAnnotation? healthCheckAnnotation = null;

            // Find the relevant health check annotation. If the resource has a parent, walk up the tree
            // until we find the health check annotation.
            while (true)
            {
                // If we find a health check annotation, break out of the loop
                if (resource.TryGetLastAnnotation(out healthCheckAnnotation))
                {
                    break;
                }

                // If the resource has a parent, walk up the tree
                if (resource is IResourceWithParent parent)
                {
                    resource = parent.Parent;
                }
                else
                {
                    break;
                }
            }

            Func<CancellationToken, ValueTask>? operation = null;

            if (healthCheckAnnotation?.HealthCheckFactory is { } factory)
            {
                IHealthCheck? check;

                try
                {
                    // TODO: Do need to pass a cancellation token here?
                    check = await factory(resource, default);

                    if (check is not null)
                    {
                        var context = new HealthCheckContext()
                        {
                            Registration = new HealthCheckRegistration("", check, HealthStatus.Unhealthy, [])
                        };

                        operation = async (cancellationToken) =>
                        {
                            var result = await check.CheckHealthAsync(context, cancellationToken);

                            if (result.Exception is not null)
                            {
                                ExceptionDispatchInfo.Throw(result.Exception);
                            }

                            if (result.Status != HealthStatus.Healthy)
                            {
                                throw new Exception("Health check failed");
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);

                    return;
                }
            }

            try
            {
                if (operation is not null)
                {
                    var pipeline = CreateResiliencyPipeline();

                    await pipeline.ExecuteAsync(operation);
                }

                tcs.TrySetResult();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }

        private static ResiliencePipeline CreateResiliencyPipeline()
        {
            var retryUntilCancelled = new RetryStrategyOptions()
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 5,
                UseJitter = true,
                MaxDelay = TimeSpan.FromSeconds(30)
            };

            return new ResiliencePipelineBuilder().AddRetry(retryUntilCancelled).Build();
        }

        public ValueTask DisposeAsync()
        {
            _cts.Cancel();
            return default;
        }
    }
}
