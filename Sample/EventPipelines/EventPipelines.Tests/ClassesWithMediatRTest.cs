﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventPipelines.MediatR;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventPipelines.Tests
{
    public class ClassesWithMediatRTest
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

        public class AdminStorage
        {
            public static readonly string[] TenantNames = { "FB", "Google", "Twitter" };

            public List<AdminGrantedInTenant> AdminsInTenants = new();
            public List<AdminAdded> GlobalAdmins = new();
        }

        public class IsAdmin: IEventFilter<UserAdded>
        {
            public ValueTask<bool> Handle(UserAdded @event, CancellationToken ct) =>
                ValueTask.FromResult(@event.IsAdmin);
        }

        public class ToAdminAdded: IEventTransformation<UserAdded, AdminAdded>
        {
            public ValueTask<AdminAdded> Handle(UserAdded @event, CancellationToken ct) =>
                ValueTask.FromResult(new AdminAdded(@event.FirstName, @event.LastName));
        }

        public class HandleAdminAdded: IEventHandler<AdminAdded>
        {
            private readonly AdminStorage adminStorage;

            public HandleAdminAdded(AdminStorage adminStorage)
            {
                this.adminStorage = adminStorage;
            }

            public ValueTask Handle(AdminAdded @event, CancellationToken ct)
            {
                adminStorage.GlobalAdmins.Add(@event);
                return ValueTask.CompletedTask;
            }
        }

        public class SendToTenants: IEventTransformation<AdminAdded, List<AdminGrantedInTenant>>
        {
            public ValueTask<List<AdminGrantedInTenant>> Handle(AdminAdded @event, CancellationToken ct) =>
                ValueTask.FromResult(AdminStorage.TenantNames
                    .Select(tenantName =>
                        new AdminGrantedInTenant(@event.FirstName, @event.LastName, tenantName)
                    )
                    .ToList());
        }

        public class HandleAdminGrantedInTenant: IEventHandler<AdminGrantedInTenant>
        {
            private readonly AdminStorage adminStorage;

            public HandleAdminGrantedInTenant(AdminStorage adminStorage)
            {
                this.adminStorage = adminStorage;
            }

            public ValueTask Handle(AdminGrantedInTenant @event, CancellationToken ct)
            {
                adminStorage.AdminsInTenants.Add(@event);
                return ValueTask.CompletedTask;
            }
        }

        private readonly IServiceProvider sp;

        public ClassesWithMediatRTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<AdminStorage>()
                .AddEventBus()
                .RouteEventsFromMediatR()
                .AddScoped<IMediator, Mediator>()
                .AddTransient<ServiceFactory>(s => t => s.GetService(t)!)
                .AddEventHandler<IsAdmin>()
                .AddEventHandler<ToAdminAdded>()
                .AddEventHandler<HandleAdminAdded>()
                .AddEventHandler<SendToTenants>()
                .AddEventHandler<HandleAdminGrantedInTenant>();

            sp = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public async Task ShouldWork()
        {
            var eventBus = sp.GetRequiredService<IMediator>();

            await eventBus.Publish(new UserAdded("Oskar", "TheGrouch", false), CancellationToken.None);

            await eventBus.Publish(new UserAdded("Big", "Bird", true), CancellationToken.None);

            var adminStorage = sp.GetRequiredService<AdminStorage>();

            adminStorage.AdminsInTenants.Should().HaveCount(3);
            adminStorage.GlobalAdmins.Should().HaveCount(1);

            adminStorage.GlobalAdmins.Single().FirstName.Should().Be("Big");
            adminStorage.GlobalAdmins.Single().LastName.Should().Be("Bird");
        }
    }
}
