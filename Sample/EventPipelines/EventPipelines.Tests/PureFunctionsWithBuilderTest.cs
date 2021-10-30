using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace EventPipelines.Tests
{
    public class PureFunctionsWithBuilderTest
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

        private readonly EventHandlersBuilder builder;

        public PureFunctionsWithBuilderTest()
        {
            builder = EventHandlersBuilder
                .Setup()
                .Filter<UserAdded>(AdminPipeline.IsAdmin)
                .Transform<UserAdded, AdminAdded>(AdminPipeline.ToAdminAdded)
                .Handle<AdminAdded>(AdminPipeline.Handle)
                .Transform<UserAdded, List<AdminGrantedInTenant>>(AdminPipeline.SendToTenants)
                .Handle<AdminGrantedInTenant>(AdminPipeline.Handle);
        }

        [Fact]
        public async Task ShouldWork()
        {
            var eventBus = new EventBus(builder.Build());

            await eventBus.Publish(new UserAdded("Oskar", "TheGrouch", false), CancellationToken.None);

            await eventBus.Publish(new UserAdded("Big", "Bird", true), CancellationToken.None);

            AdminPipeline.AdminsInTenants.Should().HaveCount(3);
            AdminPipeline.GlobalAdmins.Should().HaveCount(1);

            AdminPipeline.GlobalAdmins.Single().FirstName.Should().Be("Big");
            AdminPipeline.GlobalAdmins.Single().LastName.Should().Be("Bird");
        }
    }
}
