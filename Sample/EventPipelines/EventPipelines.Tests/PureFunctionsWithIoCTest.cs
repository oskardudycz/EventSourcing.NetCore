using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventPipelines.Tests
{
    public record UserAdded(
        string FirstName,
        string LastName,
        bool IsAdmin
    );


    public record AdminAdded(
        string FirstName,
        string LastName
    );


    public record AdminGrantedInTenant(
        string FirstName,
        string LastName,
        string TenantName
    );

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

    public class PureFunctionsTest
    {
        private readonly ServiceProvider sp;

        public PureFunctionsTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddEventBus()
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
            var eventBus = sp.GetRequiredService<IEventBus>();

            await eventBus.Publish(new UserAdded("Oskar", "TheGrouch", false), CancellationToken.None);

            await eventBus.Publish(new UserAdded("Big", "Bird", true), CancellationToken.None);

            AdminPipeline.AdminsInTenants.Should().HaveCount(3);
            AdminPipeline.GlobalAdmins.Should().HaveCount(1);

            AdminPipeline.GlobalAdmins.Single().FirstName.Should().Be("Big");
            AdminPipeline.GlobalAdmins.Single().LastName.Should().Be("Bird");
        }
    }
}
