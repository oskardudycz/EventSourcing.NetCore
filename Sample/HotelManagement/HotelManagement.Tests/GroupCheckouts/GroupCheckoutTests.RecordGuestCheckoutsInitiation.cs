using HotelManagement.GroupCheckouts;
using Ogooreck.BusinessLogic;
using Xunit;

namespace HotelManagement.Tests.GroupCheckouts;

public partial class GroupCheckoutTests
{
    [Fact]
    public void GivenNonExistingGroupCheckout_WhenRecordGuestCheckoutsInitiation_ThenIgnores()
    {
        var guestStaysIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        Spec.Given()
            .When(state => state.RecordGuestCheckoutsInitiation(guestStaysIds, now).IsPresent)
            .Then(false);
    }

    [Fact]
    public void GivenInitiatedGroupCheckout_WhenRecordGuestCheckoutsInitiation_ThenSucceeds()
    {
        var guestStaysIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        Spec.Given(new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStaysIds, now))
            .When(state => state.RecordGuestCheckoutsInitiation(guestStaysIds, now).GetOrThrow())
            .Then(new GuestCheckoutsInitiated(groupCheckoutId, guestStaysIds, now));
    }
}
