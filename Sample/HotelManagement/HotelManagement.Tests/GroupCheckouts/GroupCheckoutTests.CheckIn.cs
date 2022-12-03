using HotelManagement.GroupCheckouts;
using Ogooreck.BusinessLogic;
using Xunit;

namespace HotelManagement.Tests.GroupCheckouts;

public partial class GroupCheckoutTests
{
    [Fact]
    public void GivenNonExistingGroupCheckout_WhenInitiate_ThenSucceeds()
    {
        var guestStaysIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        Spec.Given()
            .When(_ => GroupCheckout.Initiate(groupCheckoutId, clerkId, guestStaysIds, now))
            .Then(new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStaysIds, now));
    }
}
