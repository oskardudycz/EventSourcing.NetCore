using HotelManagement.Saga.GroupCheckouts;
using Ogooreck.BusinessLogic;
using Xunit;

namespace HotelManagement.Tests.Saga.GroupCheckouts;

public partial class GroupCheckoutTests
{
    [Fact]
    public void GivenNonExistingGroupCheckout_WhenRecordGuestCheckoutFailure_ThenIgnores()
    {
        var guestStaysId = Guid.NewGuid();

        Spec.Given()
            .When(state => state.RecordGuestCheckoutFailure(guestStaysId, now).IsPresent)
            .Then(false);
    }

    [Fact]
    public void GivenInitiatedGroupCheckout_WhenRecordGuestCheckoutFailure_ThenSucceeds()
    {
        var guestStaysIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        Spec.Given(
                new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStaysIds, now),
                new GuestCheckoutsInitiated(groupCheckoutId, guestStaysIds, now)
            )
            .When(state => state.RecordGuestCheckoutFailure(guestStaysIds[0], now).GetOrThrow())
            .Then(new GuestCheckoutFailed(groupCheckoutId, guestStaysIds[0], now));
    }

    [Fact]
    public void GivenInitiatedGroupCheckout_WhenRecordGuestCheckoutFailureTwice_ThenIgnores()
    {
        var guestStaysIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        Spec.Given(
                new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStaysIds, now),
                new GuestCheckoutsInitiated(groupCheckoutId, guestStaysIds, now),
                new GuestCheckoutFailed(groupCheckoutId, guestStaysIds[0], now)
            )
            .When(state => state.RecordGuestCheckoutFailure(guestStaysIds[0], now).IsPresent)
            .Then(false);
    }

    [Fact]
    public void GivenInitiatedGroupCheckout_WhenRecordLastGuestCheckoutFailure_ThenCompletesWithFailure()
    {
        var guestStaysIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        Spec.Given(
                new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStaysIds, now),
                new GuestCheckoutsInitiated(groupCheckoutId, guestStaysIds, now),
                new GuestCheckoutFailed(groupCheckoutId, guestStaysIds[0], now),
                new GuestCheckoutFailed(groupCheckoutId, guestStaysIds[1], now)
            )
            .When(state => state.RecordGuestCheckoutFailure(guestStaysIds[2], now).GetOrThrow())
            .Then(
                new GuestCheckoutFailed(groupCheckoutId, guestStaysIds[2], now),
                new GroupCheckoutFailed(
                    groupCheckoutId,
                    Array.Empty<Guid>(),
                    guestStaysIds,
                    now
                )
            );
    }


    [Fact]
    public void GivenInitiatedGroupCheckoutWithFailure_WhenRecordLastGuestCheckoutFailure_ThenCompletesWithFailure()
    {
        var guestStaysIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        Spec.Given(
                new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStaysIds, now),
                new GuestCheckoutsInitiated(groupCheckoutId, guestStaysIds, now),
                new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[0], now),
                new GuestCheckoutFailed(groupCheckoutId, guestStaysIds[1], now)
            )
            .When(state => state.RecordGuestCheckoutFailure(guestStaysIds[2], now).GetOrThrow())
            .Then(
                new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[2], now),
                new GroupCheckoutFailed(
                    groupCheckoutId,
                    new[] { guestStaysIds[0] },
                    new[] { guestStaysIds[1], guestStaysIds[2] },
                    now
                )
            );
    }

    [Fact]
    public void GivenCompletedGroupCheckoutWithFailure_WhenRecordGuestCheckoutFailure_ThenIgnores()
    {
        var guestStaysIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        Spec.Given(
                new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStaysIds, now),
                new GuestCheckoutsInitiated(groupCheckoutId, guestStaysIds, now),
                new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[0], now),
                new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[1], now),
                new GuestCheckoutFailed(groupCheckoutId, guestStaysIds[2], now),
                new GroupCheckoutFailed(
                    groupCheckoutId,
                    new[] { guestStaysIds[0], guestStaysIds[1] },
                    new[] { guestStaysIds[2] },
                    now
                )
            )
            .When(state => state.RecordGuestCheckoutFailure(guestStaysIds[2], now).IsPresent)
            .Then(false);
    }

    [Fact]
    public void GivenCompletedGroupCheckout_WhenRecordGuestCheckoutFailure_ThenIgnores()
    {
        var guestStaysIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        Spec.Given(
                new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStaysIds, now),
                new GuestCheckoutsInitiated(groupCheckoutId, guestStaysIds, now),
                new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[0], now),
                new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[1], now),
                new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[2], now),
                new GroupCheckoutFailed(
                    groupCheckoutId,
                    new[] { guestStaysIds[0], guestStaysIds[2] },
                    new[] { guestStaysIds[1] },
                    now
                )
            )
            .When(state => state.RecordGuestCheckoutFailure(guestStaysIds[2], now).IsPresent)
            .Then(false);
    }
}
