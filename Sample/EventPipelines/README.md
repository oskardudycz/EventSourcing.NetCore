[![Twitter Follow](https://img.shields.io/twitter/follow/oskar_at_net?style=social)](https://twitter.com/oskar_at_net) [![Github Sponsors](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&link=https://github.com/sponsors/oskardudycz/)](https://github.com/sponsors/oskardudycz/) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_jvm) [![blog](https://img.shields.io/badge/%F0%9F%9A%80-Architecture%20Weekly-important)](https://www.architecture-weekly.com/?utm_source=event_sourcing_net) 

# Event Pipelines

Shows how to compose event handlers in the processing pipelines to:
- filter events,
- transform them,
- NOT requiring marker interfaces for events,
- NOT requiring marker interfaces for handlers,
- enables composition through regular functions,
- allows using interfaces and classes if you want to,
- can be used with Dependency Injection, but also without through builder,
- integrates with MediatR if you want to.
- 📝 Read more [How to build a simple event pipeline](https://event-driven.io/en/how_to_build_simple_event_pipeline/?utm_source=event_sourcing_net) 

## Overview

Having UserAdded event:

```csharp
public record UserAdded(
    string FirstName,
    string LastName,
    bool IsAdmin
);
```

We may want to create a pipeline, that will at first filter admin users:

```csharp
public static bool IsAdmin(UserAdded @event) =>
    @event.IsAdmin;
```

Then map events to a dedicated `AdminAdded` event:

```csharp
public record AdminAdded(
    string FirstName,
    string LastName
);

public static AdminAdded ToAdminAdded(UserAdded @event) =>
    new(@event.FirstName, @event.LastName);
```

Then handle mapped events storing information about new admins:

```csharp
public static void Handle(AdminAdded @event) =>
    GlobalAdmins.Add(@event);
```

And distribute global admins to all tenants:

```csharp
public static List<AdminGrantedInTenant> SendToTenants(UserAdded @event) =>
    TenantNames
        .Select(tenantName =>
            new AdminGrantedInTenant(@event.FirstName, @event.LastName, tenantName)
        )
        .ToList();

public record AdminGrantedInTenant(
    string FirstName,
    string LastName,
    string TenantName
);

public static void Handle(AdminGrantedInTenant @event) =>
    AdminsInTenants.Add(@event);
}
```

MediatR is great, but it doesn't enable such advanced pipelines. This sample shows how to construct event pipelines seamlessly. See [EventBus](./EventPipelines/EventBus.cs) implementation.

You can use it with Dependency Injection

```csharp
serviceCollection
    .AddEventBus()
    .Filter<UserAdded>(AdminPipeline.IsAdmin)
    .Transform<UserAdded, AdminAdded>(AdminPipeline.ToAdminAdded)
    .Handle<AdminAdded>(AdminPipeline.Handle)
    .Transform<UserAdded, List<AdminGrantedInTenant>>(AdminPipeline.SendToTenants)
    .Handle<AdminGrantedInTenant>(AdminPipeline.Handle);
```

or without:

```csharp
var builder = EventHandlersBuilder
    .Setup()
    .Filter<UserAdded>(AdminPipeline.IsAdmin)
    .Transform<UserAdded, AdminAdded>(AdminPipeline.ToAdminAdded)
    .Handle<AdminAdded>(AdminPipeline.Handle)
    .Transform<UserAdded, List<AdminGrantedInTenant>>(AdminPipeline.SendToTenants)
    .Handle<AdminGrantedInTenant>(AdminPipeline.Handle);

var eventBus = new EventBus(builder);
```

## Samples

Check different ways of defining and integrating Event Handlers:
- [Pure functions with Builder](./EventPipelines.Tests/PureFunctionsWithBuilderTest.cs)
- [Pure functions with Dependency Injection](./EventPipelines.Tests/PureFunctionsWithIoCTest.cs)
- [Classes with Builder](./EventPipelines.Tests/PureFunctionsWithBuilderTest.cs)
- [Classes with Dependency Injection](./EventPipelines.Tests/ClassesWithIoCTest.cs)

And how to integrate with MediatR:
- [Pure functions](./EventPipelines.Tests/PureFunctionsWithMediatRTest.cs)
- [Classes](./EventPipelines.Tests/ClassesWithMediatRTest.cs)
