using EventPipelines.MediatR;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventPipelines.Tests;

public class PureFunctionsWithMediatRTest
{
    public record UserAdded(
        string FirstName,
        string LastName,
        bool IsAdmin
    ) : INotification;

    public record AdminAdded(
        string FirstName,
        string LastName
    ) : INotification;

    public record AdminGrantedInTenant(
        string FirstName,
        string LastName,
        string TenantName
    ) : INotification;

    public static class AdminPipeline
    {
        public static readonly string[] TenantNames = { "FB", "Google", "Twitter" };

        public static List<AdminGrantedInTenant> AdminsInTenants = new();
        public static List<AdminAdded> GlobalAdmins = new();

        public static bool IsAdmin(UserAdded @event) =>
            @event.IsAdmin;

        public static AdminAdded ToAdminAdded(UserAdded @event) =>
            new(@event.FirstName, @event.LastName);

        public static void Handle(AdminAdded @event) =>
            GlobalAdmins.Add(@event);

        public static List<AdminGrantedInTenant> SendToTenants(UserAdded @event) =>
            TenantNames
                .Select(tenantName =>
                    new AdminGrantedInTenant(@event.FirstName, @event.LastName, tenantName)
                )
                .ToList();

        public static void Handle(AdminGrantedInTenant @event) =>
            AdminsInTenants.Add(@event);
    }

    private readonly IServiceProvider sp;

    public PureFunctionsWithMediatRTest()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddEventBus()
            .RouteEventsFromMediatR()
            .AddScoped<IMediator, Mediator>()
            .Filter<UserAdded>(AdminPipeline.IsAdmin)
            .Transform<UserAdded, AdminAdded>(AdminPipeline.ToAdminAdded)
            .Handle<AdminAdded>(AdminPipeline.Handle)
            .Transform<UserAdded, List<AdminGrantedInTenant>>(AdminPipeline.SendToTenants)
            .Handle<AdminGrantedInTenant>(AdminPipeline.Handle);

        sp = serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public async Task ShouldWork()
    {
        var eventBus = sp.GetRequiredService<IMediator>();

        await eventBus.Publish(new UserAdded("Oskar", "TheGrouch", false), CancellationToken.None);

        await eventBus.Publish(new UserAdded("Big", "Bird", true), CancellationToken.None);

        AdminPipeline.AdminsInTenants.Should().HaveCount(3);
        AdminPipeline.GlobalAdmins.Should().HaveCount(1);

        AdminPipeline.GlobalAdmins.Single().FirstName.Should().Be("Big");
        AdminPipeline.GlobalAdmins.Single().LastName.Should().Be("Bird");
    }
}
