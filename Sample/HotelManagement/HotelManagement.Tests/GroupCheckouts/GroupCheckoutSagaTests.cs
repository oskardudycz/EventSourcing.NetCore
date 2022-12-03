using Core.Commands;
using FluentAssertions;
using HotelManagement.GroupCheckouts;
using HotelManagement.GuestStayAccounts;
using Xunit;
using GuestCheckoutFailed = HotelManagement.GuestStayAccounts.GuestCheckoutFailed;

namespace HotelManagement.Tests.GroupCheckouts;

using static GuestCheckoutFailed;

public class GroupCheckoutSagaTests
{
    private readonly GroupCheckoutSaga saga;
    private readonly AsyncCommandBusStub commandBus = new();
    private readonly Guid groupCheckoutId = Guid.NewGuid();
    private readonly Guid clerkId = Guid.NewGuid();
    private readonly DateTimeOffset now = DateTimeOffset.UtcNow;
    private readonly CancellationToken ct = CancellationToken.None;

    public GroupCheckoutSagaTests() =>
        saga = new GroupCheckoutSaga(commandBus);

    [Fact]
    public async Task GroupCheckoutInitiated_ShouldSchedule_CheckOutGuestForEachGuesAndRecordGuestCheckoutsInitiation()
    {
        // Given
        var guestStayAccountIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var @event = new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStayAccountIds, now);

        // When
        await saga.Handle(@event, ct);

        // Then
        commandBus.ShouldHaveScheduled(
            new CheckOutGuest(guestStayAccountIds[0], groupCheckoutId),
            new CheckOutGuest(guestStayAccountIds[1], groupCheckoutId),
            new CheckOutGuest(guestStayAccountIds[2], groupCheckoutId),
            new RecordGuestCheckoutsInitiation(groupCheckoutId, guestStayAccountIds)
        );
    }

    [Fact]
    public async Task GuestCheckedOutWithGroupCheckOutId_ShouldSchedule_RecordGuestCheckoutCompletion()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        var @event = new GuestCheckedOut(guestStayAccountId, now, groupCheckoutId);

        // When
        await saga.Handle(@event, ct);

        // Then
        commandBus.ShouldHaveScheduled(
            new RecordGuestCheckoutCompletion(groupCheckoutId, guestStayAccountId, now)
        );
    }

    [Fact]
    public async Task GuestCheckedOutWithoutGroupCheckOutId_ShouldNotSchedule_RecordsGuestCheckoutCompletion()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        var @event = new GuestCheckedOut(guestStayAccountId, now);

        // When
        await saga.Handle(@event, ct);

        // Then
        commandBus.ShouldNotHaveScheduledCommands(
            new RecordGuestCheckoutCompletion(groupCheckoutId, guestStayAccountId, now)
        );
    }

    [Fact]
    public async Task GuestCheckoutFailedWithGroupCheckOutId_ShouldSchedule_RecordGuestCheckoutFailure()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        var @event = new GuestCheckoutFailed(guestStayAccountId, FailureReason.BalanceNotSettled, now, groupCheckoutId);

        // When
        await saga.Handle(@event, ct);

        // Then
        commandBus.ShouldHaveScheduled(
            new RecordGuestCheckoutFailure(groupCheckoutId, guestStayAccountId, now)
        );
    }

    [Fact]
    public async Task GuestCheckoutFailedWithoutGroupCheckOutId_ShouldNotSchedule_RecordGuestCheckoutFailure()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        var @event = new GuestCheckoutFailed(guestStayAccountId, FailureReason.BalanceNotSettled, now);

        // When
        await saga.Handle(@event, ct);

        // Then
        commandBus.ShouldNotHaveScheduledCommands(
            new RecordGuestCheckoutFailure(groupCheckoutId, guestStayAccountId, now)
        );
    }
}

internal class AsyncCommandBusStub: IAsyncCommandBus
{
    private readonly List<object> commands = new();

    public Task Schedule<TCommand>(TCommand command, CancellationToken ct = default) where TCommand : notnull
    {
        commands.Add(command);

        return Task.CompletedTask;
    }

    public void ShouldNotHaveScheduledCommands(params object[] expected)
    {
        commands.Should().NotContain(expected);
    }

    public void ShouldHaveScheduled(params object[] expected)
    {
        commands.Should().Contain(expected);
    }
}
