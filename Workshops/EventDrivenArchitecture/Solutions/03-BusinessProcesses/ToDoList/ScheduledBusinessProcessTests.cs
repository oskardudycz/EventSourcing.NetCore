using BusinessProcesses.Core;
using BusinessProcesses.ToDoList.GroupCheckouts;
using BusinessProcesses.ToDoList.GuestStayAccounts;
using Shouldly;
using Xunit;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.ToDoList;

using static GuestStayAccountEvent;
using static GuestStayAccountCommand;
using static GroupCheckoutCommand;
using static GuestStayAccountsConfig;

public class ScheduledBusinessProcessTests
{
    [Fact]
    public async Task GroupCheckoutForMultipleGuestStay_ShouldCompleteOnceTimeHasPassed()
    {
        // Given
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid()];

        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[0], now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[1], now.AddDays(-1)), TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();

        // When
        await groupCheckoutFacade.InitiateGroupCheckout(new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays, now), TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays, now),
                new CheckOutGuest(guestStays[0], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[0], now, groupCheckoutId),
                new CheckOutGuest(guestStays[1], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[1], now, groupCheckoutId),
            ]
        );

        // When
        await scheduledToDoList.HandleAll(TestContext.Current.CancellationToken);
        await scheduledToDoList.HandleAll(TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays, now),
                new CheckOutGuest(guestStays[0], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[0], now, groupCheckoutId),
                new CheckOutGuest(guestStays[1], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[1], now, groupCheckoutId),
                new GroupCheckoutEvent.GroupCheckoutCompleted(groupCheckoutId, guestStays, now),
            ]
        );
        (await database.Get<GroupCheckout>(groupCheckoutId, TestContext.Current.CancellationToken))
            .ShouldBeNull("done group checkout is removed from the to-do list");
    }

    private readonly Database database = new();
    private readonly MessageBus messageBus = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GuestStayAccountFacade guestStayFacade;
    private readonly GroupCheckoutFacade groupCheckoutFacade;
    private readonly ScheduledGroupCheckoutToDoList scheduledToDoList;
    private readonly DateTimeOffset now = DateTimeOffset.Now;

    public ScheduledBusinessProcessTests()
    {
        guestStayFacade = new GuestStayAccountFacade(database, messageBus);
        groupCheckoutFacade = new GroupCheckoutFacade(messageBus);

        messageBus.Use(publishedMessages.Catch);

        var groupCheckoutProjection = new GroupCheckoutProjection(database);
        foreach (var eventType in groupCheckoutProjection.CanHandle())
            messageBus.Subscribe(eventType, async (@event, ct) => await groupCheckoutProjection.Handle(@event, ct));
        scheduledToDoList = new ScheduledGroupCheckoutToDoList(
            database,
            new GroupCheckoutToDoList(groupCheckoutProjection, messageBus)
        );
        ConfigureGuestStayAccounts(messageBus, guestStayFacade);
    }
}
