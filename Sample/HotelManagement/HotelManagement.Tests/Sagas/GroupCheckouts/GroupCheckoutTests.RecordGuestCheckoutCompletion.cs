using HotelManagement.Sagas.GroupCheckouts;
using Ogooreck.BusinessLogic;
using Xunit;

namespace HotelManagement.Tests.Sagas.GroupCheckouts;

public partial class GroupCheckoutTests
{
    [Fact]
    public void GivenNonExistingGroupCheckout_WhenRecordGuestCheckoutCompletion_ThenIgnores()
    {
        var guestStaysId = Guid.NewGuid();

        Spec.Given()
            .When(state => state.RecordGuestCheckoutCompletion(guestStaysId, now).IsPresent)
            .Then(false);
    }

    [Fact]
    public void GivenInitiatedGroupCheckout_WhenRecordGuestCheckoutCompletion_ThenSucceeds()
    {
        var guestStaysIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        Spec.Given(
                new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStaysIds, now),
                new GuestCheckoutsInitiated(groupCheckoutId, guestStaysIds, now)
            )
            .When(state => state.RecordGuestCheckoutCompletion(guestStaysIds[0], now).GetOrThrow())
            .Then(new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[0], now));
    }

    [Fact]
    public void GivenInitiatedGroupCheckout_WhenRecordGuestCheckoutCompletionTwice_ThenIgnores()
    {
        var guestStaysIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        Spec.Given(
                new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStaysIds, now),
                new GuestCheckoutsInitiated(groupCheckoutId, guestStaysIds, now),
                new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[0], now)
            )
            .When(state => state.RecordGuestCheckoutCompletion(guestStaysIds[0], now).IsPresent)
            .Then(false);
    }

    [Fact]
    public void GivenInitiatedGroupCheckout_WhenRecordLastGuestCheckoutCompletion_ThenCompletes()
    {
        var guestStaysIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        Spec.Given(
                new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStaysIds, now),
                new GuestCheckoutsInitiated(groupCheckoutId, guestStaysIds, now),
                new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[0], now),
                new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[1], now)
            )
            .When(state => state.RecordGuestCheckoutCompletion(guestStaysIds[2], now).GetOrThrow())
            .Then(
                new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[2], now),
                new GroupCheckoutCompleted(groupCheckoutId, guestStaysIds, now)
            );
    }


    [Fact]
    public void GivenInitiatedGroupCheckoutWithFailure_WhenRecordLastGuestCheckoutCompletion_ThenCompletesWithFailure()
    {
        var guestStaysIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        Spec.Given(
                new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStaysIds, now),
                new GuestCheckoutsInitiated(groupCheckoutId, guestStaysIds, now),
                new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[0], now),
                new GuestCheckoutFailed(groupCheckoutId, guestStaysIds[1], now)
            )
            .When(state => state.RecordGuestCheckoutCompletion(guestStaysIds[2], now).GetOrThrow())
            .Then(
                new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[2], now),
                new GroupCheckoutFailed(
                    groupCheckoutId,
                    [guestStaysIds[0], guestStaysIds[2]],
                    [guestStaysIds[1]],
                    now
                )
            );
    }

    [Fact]
    public void GivenCompletedGroupCheckoutWithFailure_WhenRecordGuestCheckoutCompletion_ThenIgnores()
    {
        var guestStaysIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        Spec.Given(
                new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStaysIds, now),
                new GuestCheckoutsInitiated(groupCheckoutId, guestStaysIds, now),
                new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[0], now),
                new GuestCheckoutFailed(groupCheckoutId, guestStaysIds[1], now),
                new GuestCheckoutCompleted(groupCheckoutId, guestStaysIds[2], now),
                new GroupCheckoutFailed(
                    groupCheckoutId,
                    [guestStaysIds[0], guestStaysIds[2]],
                    [guestStaysIds[1]],
                    now
                )
            )
            .When(state => state.RecordGuestCheckoutCompletion(guestStaysIds[2], now).IsPresent)
            .Then(false);
    }

    [Fact]
    public void GivenCompletedGroupCheckout_WhenRecordGuestCheckoutCompletion_ThenIgnores()
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
                    [guestStaysIds[0], guestStaysIds[2]],
                    [guestStaysIds[1]],
                    now
                )
            )
            .When(state => state.RecordGuestCheckoutCompletion(guestStaysIds[2], now).IsPresent)
            .Then(false);
    }
}
