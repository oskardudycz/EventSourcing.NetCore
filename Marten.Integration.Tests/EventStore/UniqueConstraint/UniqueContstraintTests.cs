using JasperFx;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten.Events.Aggregation;
using Marten.Exceptions;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.EventStore.UniqueConstraint;

public class UniqueContstraintTests(MartenFixture fixture): MartenTest(fixture.PostgreSqlContainer)
{
    public record UserCreated(
        string UserId,
        string Email
    );

    public record UserEmailUpdated(
        string UserId,
        string Email
    );

    public record UserDeleted(
        string UserId
    );

    public record UserNameGuard(
        string Id,
        string Email
    );

    public class UserNameGuardProjection: SingleStreamProjection<UserNameGuard, string>
    {
        public UserNameGuardProjection() =>
            DeleteEvent<UserDeleted>();

        public UserNameGuard Create(UserCreated @event) =>
            new(@event.UserId, @event.Email);

        public UserNameGuard Apply(UserEmailUpdated @event, UserNameGuard guard) =>
            guard with { Email = @event.Email };
    }

    protected override IDocumentSession CreateSession(Action<StoreOptions>? setStoreOptions = null)
    {
        var store = DocumentStore.For(options =>
        {
            options.Connection(ConnectionString);
            options.UseNewtonsoftForSerialization(nonPublicMembersStorage: NonPublicMembersStorage.All);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.DatabaseSchemaName = SchemaName;
            options.Events.DatabaseSchemaName = SchemaName;

            options.Events.StreamIdentity = StreamIdentity.AsString;

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
        var user1Id = GenerateRandomId();
        var user1Created = new UserCreated(user1Id, email);
        EventStore.StartStream(user1Id, user1Created);
        await Session.SaveChangesAsync();

        // should succeed for user with other name
        var user2Id = GenerateRandomId();
        EventStore.StartStream(user2Id, new UserCreated(user2Id, "some.other@email.com"));
        await Session.SaveChangesAsync();

        // should fail if added again
        var user3Id = GenerateRandomId();
        EventStore.StartStream(user3Id, new UserCreated(user3Id, email));
        await SaveChangesAsyncShouldFailWith<DocumentAlreadyExistsException>();

        // should fail if updated to other existing
        EventStore.Append(user1Id, new UserEmailUpdated(GenerateRandomId(), "some.other@email.com"));
        await SaveChangesAsyncShouldFailWith<DocumentAlreadyExistsException>();

        // should succeed if updated to yet another non-existing email
        EventStore.Append(user1Id, new UserEmailUpdated(GenerateRandomId(), "yet.another@email.com"));
        await Session.SaveChangesAsync();

        // should fail for user with updated email
        var user4Id = GenerateRandomId();
        EventStore.StartStream(user4Id, new UserCreated(user4Id, "yet.another@email.com"));
        await SaveChangesAsyncShouldFailWith<DocumentAlreadyExistsException>();

        EventStore.Append(user1Id, new UserDeleted(user1Id));
        await Session.SaveChangesAsync();

        // should succeed as we deleted
        var user5Id = GenerateRandomId();
        EventStore.StartStream(user3Id, new UserCreated(user5Id, "yet.another@email.com"));
        await Session.SaveChangesAsync();
    }
}
