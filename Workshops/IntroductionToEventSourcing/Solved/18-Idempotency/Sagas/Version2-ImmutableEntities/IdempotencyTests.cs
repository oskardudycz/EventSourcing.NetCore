using Bogus;
using Idempotency.Core;
using Idempotency.Sagas.Version2_ImmutableEntities.GroupCheckouts;
using Idempotency.Sagas.Version2_ImmutableEntities.GuestStayAccounts;
using Xunit;
using Xunit.Abstractions;
using Database = Idempotency.Core.Database;
using GroupCheckoutCommand = Idempotency.Sagas.Version2_ImmutableEntities.GroupCheckouts.GroupCheckoutCommand;

namespace Idempotency.Sagas.Version2_ImmutableEntities;

using static GroupCheckoutCommand;
using static GroupCheckoutEvent;

public class IdempotencyTests
{
    [Fact]
    public async Task GroupCheckoutIsInitiatedOnceForTheSameId()
    {
        // Given
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        var command = new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays, now);
        // And
        await groupCheckoutFacade.InitiateGroupCheckout(command);

        // When
        await groupCheckoutFacade.InitiateGroupCheckout(command);

        // Then

        publishedMessages.ShouldReceiveSingleMessage(new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays,
            now));
    }

    [Fact]
    public async Task GroupCheckoutRecordsOnceGuestCheckoutCompletion()
    {
        // Given
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        // And
        await groupCheckoutFacade.InitiateGroupCheckout(new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays,
            now.AddDays(-1)));
        publishedMessages.Reset();


        var command = new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[0], now);
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(command);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(command);

        // Then

        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckoutCompleted(groupCheckoutId, guestStays[0], now));
    }

    [Fact]
    public async Task GroupCheckoutRecordsOnceGuestCheckoutFailure()
    {
        // Given
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        // And
        await groupCheckoutFacade.InitiateGroupCheckout(new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays,
            now.AddDays(-1)));
        publishedMessages.Reset();


        var command = new RecordGuestCheckoutFailure(groupCheckoutId, guestStays[0], now);
        await groupCheckoutFacade.RecordGuestCheckoutFailure(command);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutFailure(command);

        // Then

        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckOutFailed(groupCheckoutId, guestStays[0], now));
    }

    private readonly Database database = new();
    private readonly EventBus eventBus = new();
    private readonly CommandBus commandBus = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GuestStayFacade guestStayFacade;
    private readonly GroupCheckOutFacade groupCheckoutFacade;
    private readonly Faker generate = new();
    private readonly DateTimeOffset now = DateTimeOffset.Now;
    private readonly ITestOutputHelper testOutputHelper;

    public IdempotencyTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        guestStayFacade = new GuestStayFacade(database, eventBus);
        groupCheckoutFacade = new GroupCheckOutFacade(database, eventBus);

        eventBus.Use(publishedMessages.Catch);
        commandBus.Use(publishedMessages.Catch);
    }
}
