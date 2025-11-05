using Bogus;
using BusinessProcesses.Core;
using BusinessProcesses.Version2_ImmutableEntities.GroupCheckouts;
using BusinessProcesses.Version2_ImmutableEntities.GuestStayAccounts;
using Xunit;
using Xunit.Abstractions;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.Version2_ImmutableEntities;

using static GuestStayAccountCommand;
using static GuestStayAccountEvent;
using static GroupCheckoutCommand;

public class BusinessProcessTests
{
    // TODO: For each WIP test, fill out the implementation to pass the test, then remove the Trait
    [Fact, Trait("Category", "Wip")]
    public async Task GroupCheckoutForMultipleGuestStayWithoutPaymentsAndCharges_ShouldComplete()
    {
        // Given;
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];

        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[0], now.AddDays(-1)));
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[1], now.AddDays(-1)));
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[2], now.AddDays(-1)));
        publishedMessages.Reset();
        // And
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        var command = new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays, now);

        // When
        await guestStayFacade.InitiateGroupCheckout(command);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays, now),
                new CheckOutGuest(guestStays[0], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[0], now, groupCheckoutId),
                new GroupCheckoutEvent.GuestCheckoutCompleted(groupCheckoutId, guestStays[0], now),
                new CheckOutGuest(guestStays[1], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[1], now, groupCheckoutId),
                new GroupCheckoutEvent.GuestCheckoutCompleted(groupCheckoutId, guestStays[1], now),
                new CheckOutGuest(guestStays[2], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[2], now, groupCheckoutId),
                new GroupCheckoutEvent.GuestCheckoutCompleted(groupCheckoutId, guestStays[2], now),
                new GroupCheckoutEvent.GroupCheckoutCompleted(groupCheckoutId, guestStays, now),
            ]
        );
    }

    [Fact, Trait("Category", "Wip")]
    public async Task GroupCheckoutForMultipleGuestStayWithAllStaysSettled_ShouldComplete()
    {
        // Given;
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        decimal[] amounts = [generate.Finance.Amount(), generate.Finance.Amount(), generate.Finance.Amount()];

        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[0], now.AddDays(-1)));
        await guestStayFacade.RecordCharge(new RecordCharge(guestStays[0], amounts[0], now.AddHours(-2)));
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[0], amounts[0], now.AddHours(-1)));

        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[1], now.AddDays(-1)));
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[1], amounts[1], now.AddHours(-1)));
        await guestStayFacade.RecordCharge(new RecordCharge(guestStays[1], amounts[1], now.AddHours(-2)));

        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[2], now.AddDays(-1)));
        await guestStayFacade.RecordCharge(new RecordCharge(guestStays[0], amounts[2], now.AddHours(-2)));
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[0], amounts[2] / 2, now.AddHours(-1)));
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[0], amounts[2] / 2, now.AddHours(-1)));
        publishedMessages.Reset();
        // And
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        var command = new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays, now);

        // When
        await guestStayFacade.InitiateGroupCheckout(command);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays, now),
                new CheckOutGuest(guestStays[0], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[0], now, groupCheckoutId),
                new GroupCheckoutEvent.GuestCheckoutCompleted(groupCheckoutId, guestStays[0], now),
                new CheckOutGuest(guestStays[1], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[1], now, groupCheckoutId),
                new GroupCheckoutEvent.GuestCheckoutCompleted(groupCheckoutId, guestStays[1], now),
                new CheckOutGuest(guestStays[2], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[2], now, groupCheckoutId),
                new GroupCheckoutEvent.GuestCheckoutCompleted(groupCheckoutId, guestStays[2], now),
                new GroupCheckoutEvent.GroupCheckoutCompleted(groupCheckoutId, guestStays, now),
            ]
        );
    }

    [Fact, Trait("Category", "Wip")]
    public async Task GroupCheckoutForMultipleGuestStayWithOneSettledAndRestUnsettled_ShouldFail()
    {
        // Given;
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        decimal[] amounts = [generate.Finance.Amount(), generate.Finance.Amount(), generate.Finance.Amount()];

        // 🟢 settled
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[0], now.AddDays(-1)));
        await guestStayFacade.RecordCharge(new RecordCharge(guestStays[0], amounts[0], now.AddHours(-2)));
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[0], amounts[0], now.AddHours(-1)));

        // 🛑 payment without charge
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[1], now.AddDays(-1)));
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[1], amounts[1], now.AddHours(-1)));

        // 🛑 payment without charge
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[2], now.AddDays(-1)));
        await guestStayFacade.RecordCharge(new RecordCharge(guestStays[2], amounts[2], now.AddHours(-2)));
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[2], amounts[2] / 2, now.AddHours(-1)));
        publishedMessages.Reset();
        // And
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        var command = new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays, now);

        // When
        await guestStayFacade.InitiateGroupCheckout(command);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays, now),
                new CheckOutGuest(guestStays[0], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[0], now, groupCheckoutId),
                new GroupCheckoutEvent.GuestCheckoutCompleted(groupCheckoutId, guestStays[0], now),
                new CheckOutGuest(guestStays[1], now, groupCheckoutId),
                new GuestCheckOutFailed(guestStays[1], GuestCheckOutFailed.FailureReason.BalanceNotSettled, now,
                    groupCheckoutId),
                new GroupCheckoutEvent.GuestCheckOutFailed(groupCheckoutId, guestStays[1], now),
                new CheckOutGuest(guestStays[2], now, groupCheckoutId),
                new GuestCheckOutFailed(guestStays[2], GuestCheckOutFailed.FailureReason.BalanceNotSettled, now,
                    groupCheckoutId),
                new GroupCheckoutEvent.GuestCheckOutFailed(groupCheckoutId, guestStays[2], now),
                new GroupCheckoutEvent.GroupCheckoutFailed(
                    groupCheckoutId,
                    [guestStays[0]],
                    [guestStays[1], guestStays[2]],
                    now
                ),
            ]
        );
    }

    [Fact, Trait("Category", "Wip")]
    public async Task GroupCheckoutForMultipleGuestStayWithAllUnsettled_ShouldFail()
    {
        // Given;
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        decimal[] amounts = [generate.Finance.Amount(), generate.Finance.Amount(), generate.Finance.Amount()];

        // 🛑 charge without payment
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[0], now.AddDays(-1)));
        await guestStayFacade.RecordCharge(new RecordCharge(guestStays[0], amounts[0], now.AddHours(-2)));

        // 🛑 payment without charge
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[1], now.AddDays(-1)));
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[1], amounts[1], now.AddHours(-1)));

        // 🛑 payment without charge
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[2], now.AddDays(-1)));
        await guestStayFacade.RecordCharge(new RecordCharge(guestStays[2], amounts[2], now.AddHours(-2)));
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[2], amounts[2] / 2, now.AddHours(-1)));
        publishedMessages.Reset();
        // And
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        var command = new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays, now);

        // When
        await guestStayFacade.InitiateGroupCheckout(command);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays, now),
                new CheckOutGuest(guestStays[0], now, groupCheckoutId),
                new GuestCheckOutFailed(guestStays[0], GuestCheckOutFailed.FailureReason.BalanceNotSettled, now,
                    groupCheckoutId),
                new GroupCheckoutEvent.GuestCheckOutFailed(groupCheckoutId, guestStays[0], now),
                new CheckOutGuest(guestStays[1], now, groupCheckoutId),
                new GuestCheckOutFailed(guestStays[1], GuestCheckOutFailed.FailureReason.BalanceNotSettled, now,
                    groupCheckoutId),
                new GroupCheckoutEvent.GuestCheckOutFailed(groupCheckoutId, guestStays[1], now),
                new CheckOutGuest(guestStays[2], now, groupCheckoutId),
                new GuestCheckOutFailed(guestStays[2], GuestCheckOutFailed.FailureReason.BalanceNotSettled, now,
                    groupCheckoutId),
                new GroupCheckoutEvent.GuestCheckOutFailed(groupCheckoutId, guestStays[2], now),
                new GroupCheckoutEvent.GroupCheckoutFailed(
                    groupCheckoutId,
                    [],
                    [guestStays[0], guestStays[1], guestStays[2]],
                    now
                ),
            ]
        );
    }

    private readonly EventStore eventStore = new();
    private readonly CommandBus commandBus = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GuestStayFacade guestStayFacade;
    private readonly Faker generate = new();
    private readonly DateTimeOffset now = DateTimeOffset.Now;
    private readonly ITestOutputHelper testOutputHelper;

    public BusinessProcessTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        guestStayFacade = new GuestStayFacade(eventStore);

        eventStore.Use(publishedMessages.Catch);
    }
}
