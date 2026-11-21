using Bogus;
using OptimisticConcurrency.Core;
using OptimisticConcurrency.RetryInInfrastructure.Version1_Aggregates.GroupCheckouts;
using OptimisticConcurrency.RetryInInfrastructure.Version1_Aggregates.GuestStayAccounts;
using Xunit;
using Database = OptimisticConcurrency.Core.Database;
using GroupCheckoutCommand = OptimisticConcurrency.RetryInInfrastructure.Version1_Aggregates.GroupCheckouts.GroupCheckoutCommand;

namespace OptimisticConcurrency.RetryInInfrastructure.Version1_Aggregates;

using static GuestStayAccountEvent;
using static GuestStayAccountCommand;
using static GroupCheckoutsConfig;
using static GuestStayAccountsConfig;
using static GroupCheckoutCommand;

public class BusinessProcessTests
{
    [Fact]
    public async Task GroupCheckoutForMultipleGuestStayWithoutPaymentsAndCharges_ShouldComplete()
    {
        // Given;
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];

        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[0], now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[1], now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[2], now.AddDays(-1)), TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        var command = new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays, now);

        // When
        await groupCheckoutFacade.InitiateGroupCheckout(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays, now),
                new CheckOutGuest(guestStays[0], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[0], now, groupCheckoutId),
                new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[0], now),
                new GroupCheckoutEvent.GuestCheckoutCompletionRecorded(groupCheckoutId, guestStays[0], now),
                new CheckOutGuest(guestStays[1], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[1], now, groupCheckoutId),
                new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[1], now),
                new GroupCheckoutEvent.GuestCheckoutCompletionRecorded(groupCheckoutId, guestStays[1], now),
                new CheckOutGuest(guestStays[2], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[2], now, groupCheckoutId),
                new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[2], now),
                new GroupCheckoutEvent.GuestCheckoutCompletionRecorded(groupCheckoutId, guestStays[2], now),
                new GroupCheckoutEvent.GroupCheckoutCompleted(groupCheckoutId, guestStays, now),
            ]
        );
    }

    [Fact]
    public async Task GroupCheckoutForMultipleGuestStayWithAllStaysSettled_ShouldComplete()
    {
        // Given;
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        decimal[] amounts = [generate.Finance.Amount(), generate.Finance.Amount(), generate.Finance.Amount()];

        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[0], now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordCharge(new RecordCharge(guestStays[0], Guid.NewGuid(), amounts[0], now.AddHours(-2)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[0], Guid.NewGuid(), amounts[0], now.AddHours(-1)), TestContext.Current.CancellationToken);

        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[1], now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[1], Guid.NewGuid(), amounts[1], now.AddHours(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordCharge(new RecordCharge(guestStays[1], Guid.NewGuid(), amounts[1], now.AddHours(-2)), TestContext.Current.CancellationToken);

        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[2], now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordCharge(new RecordCharge(guestStays[0], Guid.NewGuid(), amounts[2], now.AddHours(-2)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[0], Guid.NewGuid(), amounts[2] / 2, now.AddHours(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[0], Guid.NewGuid(), amounts[2] / 2, now.AddHours(-1)), TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        var command = new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays, now);

        // When
        await groupCheckoutFacade.InitiateGroupCheckout(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays, now),
                new CheckOutGuest(guestStays[0], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[0], now, groupCheckoutId),
                new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[0], now),
                new GroupCheckoutEvent.GuestCheckoutCompletionRecorded(groupCheckoutId, guestStays[0], now),
                new CheckOutGuest(guestStays[1], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[1], now, groupCheckoutId),
                new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[1], now),
                new GroupCheckoutEvent.GuestCheckoutCompletionRecorded(groupCheckoutId, guestStays[1], now),
                new CheckOutGuest(guestStays[2], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[2], now, groupCheckoutId),
                new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[2], now),
                new GroupCheckoutEvent.GuestCheckoutCompletionRecorded(groupCheckoutId, guestStays[2], now),
                new GroupCheckoutEvent.GroupCheckoutCompleted(groupCheckoutId, guestStays, now),
            ]
        );
    }

    [Fact]
    public async Task GroupCheckoutForMultipleGuestStayWithOneSettledAndRestUnsettled_ShouldFail()
    {
        // Given;
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        decimal[] amounts = [generate.Finance.Amount(), generate.Finance.Amount(), generate.Finance.Amount()];

        // 🟢 settled
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[0], now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordCharge(new RecordCharge(guestStays[0], Guid.NewGuid(), amounts[0], now.AddHours(-2)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[0], Guid.NewGuid(), amounts[0], now.AddHours(-1)), TestContext.Current.CancellationToken);

        // 🛑 payment without charge
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[1], now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[1], Guid.NewGuid(), amounts[1], now.AddHours(-1)), TestContext.Current.CancellationToken);

        // 🛑 payment without charge
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[2], now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordCharge(new RecordCharge(guestStays[2], Guid.NewGuid(), amounts[2], now.AddHours(-2)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[2], Guid.NewGuid(), amounts[2] / 2, now.AddHours(-1)), TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        var command = new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays, now);

        // When
        await groupCheckoutFacade.InitiateGroupCheckout(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays, now),
                new CheckOutGuest(guestStays[0], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[0], now, groupCheckoutId),
                new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[0], now),
                new GroupCheckoutEvent.GuestCheckoutCompletionRecorded(groupCheckoutId, guestStays[0], now),
                new CheckOutGuest(guestStays[1], now, groupCheckoutId),
                new GuestCheckoutFailed(guestStays[1], GuestCheckoutFailed.FailureReason.BalanceNotSettled, now,
                    groupCheckoutId),
                new RecordGuestCheckoutFailure(groupCheckoutId, guestStays[1], now),
                new GroupCheckoutEvent.GuestCheckoutFailureRecorded(groupCheckoutId, guestStays[1], now),
                new CheckOutGuest(guestStays[2], now, groupCheckoutId),
                new GuestCheckoutFailed(guestStays[2], GuestCheckoutFailed.FailureReason.BalanceNotSettled, now,
                    groupCheckoutId),
                new RecordGuestCheckoutFailure(groupCheckoutId, guestStays[2], now),
                new GroupCheckoutEvent.GuestCheckoutFailureRecorded(groupCheckoutId, guestStays[2], now),
                new GroupCheckoutEvent.GroupCheckoutFailed(
                    groupCheckoutId,
                    [guestStays[0]],
                    [guestStays[1], guestStays[2]],
                    now
                ),
            ]
        );
    }


    [Fact]
    public async Task GroupCheckoutForMultipleGuestStayWithAllUnsettled_ShouldFail()
    {
        // Given;
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        decimal[] amounts = [generate.Finance.Amount(), generate.Finance.Amount(), generate.Finance.Amount()];

        // 🛑 charge without payment
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[0], now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordCharge(new RecordCharge(guestStays[0], Guid.NewGuid(), amounts[0], now.AddHours(-2)), TestContext.Current.CancellationToken);

        // 🛑 payment without charge
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[1], now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[1], Guid.NewGuid(), amounts[1], now.AddHours(-1)), TestContext.Current.CancellationToken);

        // 🛑 payment without charge
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[2], now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordCharge(new RecordCharge(guestStays[2], Guid.NewGuid(), amounts[2], now.AddHours(-2)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordPayment(new RecordPayment(guestStays[2], Guid.NewGuid(), amounts[2] / 2, now.AddHours(-1)), TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        var command = new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays, now);

        // When
        await groupCheckoutFacade.InitiateGroupCheckout(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays, now),
                new CheckOutGuest(guestStays[0], now, groupCheckoutId),
                new GuestCheckoutFailed(guestStays[0], GuestCheckoutFailed.FailureReason.BalanceNotSettled, now,
                    groupCheckoutId),
                new RecordGuestCheckoutFailure(groupCheckoutId, guestStays[0], now),
                new GroupCheckoutEvent.GuestCheckoutFailureRecorded(groupCheckoutId, guestStays[0], now),
                new CheckOutGuest(guestStays[1], now, groupCheckoutId),
                new GuestCheckoutFailed(guestStays[1], GuestCheckoutFailed.FailureReason.BalanceNotSettled, now,
                    groupCheckoutId),
                new RecordGuestCheckoutFailure(groupCheckoutId, guestStays[1], now),
                new GroupCheckoutEvent.GuestCheckoutFailureRecorded(groupCheckoutId, guestStays[1], now),
                new CheckOutGuest(guestStays[2], now, groupCheckoutId),
                new GuestCheckoutFailed(guestStays[2], GuestCheckoutFailed.FailureReason.BalanceNotSettled, now,
                    groupCheckoutId),
                new RecordGuestCheckoutFailure(groupCheckoutId, guestStays[2], now),
                new GroupCheckoutEvent.GuestCheckoutFailureRecorded(groupCheckoutId, guestStays[2], now),
                new GroupCheckoutEvent.GroupCheckoutFailed(
                    groupCheckoutId,
                    [],
                    [guestStays[0], guestStays[1], guestStays[2]],
                    now
                ),
            ]
        );
    }

    private readonly Database database = new();
    private readonly MessageBus messageBus = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GuestStayAccountFacade guestStayFacade;
    private readonly GroupCheckoutFacade groupCheckoutFacade;
    private readonly Faker generate = new();
    private readonly DateTimeOffset now = DateTimeOffset.Now;
    private readonly ITestOutputHelper testOutputHelper;

    public BusinessProcessTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        guestStayFacade = new GuestStayAccountFacade(database, messageBus);
        groupCheckoutFacade = new GroupCheckoutFacade(database, messageBus);

        messageBus.Use(publishedMessages.Catch);

        ConfigureGroupCheckouts(messageBus, groupCheckoutFacade);
        ConfigureGuestStayAccounts(messageBus, guestStayFacade);
    }
}
