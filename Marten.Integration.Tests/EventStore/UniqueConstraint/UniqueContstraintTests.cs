using FluentAssertions;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using Marten.Exceptions;
using Marten.Integration.Tests.TestsInfrastructure;
using Weasel.Core;
using Xunit;

namespace Marten.Integration.Tests.EventStore.UniqueConstraint;

public class UniqueContstraintTests: MartenTest
{
    public record UserCreated(
        Guid UserId,
        string Email
    );

    public record UserEmailUpdated(
        Guid UserId,
        string Email
    );

    public record UserDeleted(
        Guid UserId
    );

    public record UserNameGuard(
        Guid Id,
        string Email
    );

    public class UserNameGuardProjection: SingleStreamAggregation<UserNameGuard>
    {
        public UserNameGuardProjection() =>
            DeleteEvent<UserDeleted>();

        public UserNameGuard Create(UserCreated @event) =>
            new (@event.UserId, @event.Email);

        public UserNameGuard Apply(UserEmailUpdated @event, UserNameGuard guard) =>
            guard with { Email = @event.Email };
    }

    protected override IDocumentSession CreateSession(Action<StoreOptions>? setStoreOptions = null)
    {
        var store = DocumentStore.For(options =>
        {
            options.Connection(Settings.ConnectionString);
            options.UseDefaultSerialization(nonPublicMembersStorage: NonPublicMembersStorage.All);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.DatabaseSchemaName = SchemaName;
            options.Events.DatabaseSchemaName = SchemaName;

            options.Projections.Add<UserNameGuardProjection>(ProjectionLifecycle.Inline);

            options.Schema.For<UserNameGuard>().UniqueIndex(guard => guard.Email);
        });

        return store.LightweightSession();
    }

    [Fact]
    public async Task GivenEvents_WhenInlineTransformationIsApplied_ThenReturnsSameNumberOfTransformedItems()
    {
        // Create user 1
        var email = "john.doe@gmail.com";
        var user1Created = new UserCreated(Guid.NewGuid(), email);
        var user1Id = EventStore.StartStream(user1Created).Id;
        await Session.SaveChangesAsync();

        // should succeed for user with other name
        EventStore.StartStream(new UserCreated(Guid.NewGuid(), "some.other@email.com"));
        await Session.SaveChangesAsync();

        // should fail if added again
        EventStore.StartStream(new UserCreated(Guid.NewGuid(), email));
        await SaveChangesAsyncShouldFailWith<DocumentAlreadyExistsException>();

        // should fail if updated to other existing
        EventStore.Append(user1Id, new UserEmailUpdated(Guid.NewGuid(), "some.other@email.com"));
        await SaveChangesAsyncShouldFailWith<DocumentAlreadyExistsException>();

        // should succeed if updated to yet another non-existing email
        EventStore.Append(user1Id, new UserEmailUpdated(Guid.NewGuid(), "yet.another@email.com"));
        await Session.SaveChangesAsync();

        // should fail for user with updated email
        EventStore.StartStream(new UserCreated(Guid.NewGuid(), "yet.another@email.com"));
        await SaveChangesAsyncShouldFailWith<DocumentAlreadyExistsException>();

        EventStore.Append(user1Id, new UserDeleted(user1Id));
        await Session.SaveChangesAsync();

        // should succeed as we deleted
        EventStore.StartStream(new UserCreated(Guid.NewGuid(), "yet.another@email.com"));
        await Session.SaveChangesAsync();
    }
}
