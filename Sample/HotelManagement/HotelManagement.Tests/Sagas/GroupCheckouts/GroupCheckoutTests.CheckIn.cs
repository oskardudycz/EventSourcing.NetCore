using HotelManagement.Sagas.GroupCheckouts;
using Ogooreck.BusinessLogic;
using Xunit;

namespace HotelManagement.Tests.Sagas.GroupCheckouts;

public partial class GroupCheckoutTests
{
    [Fact]
    public void GivenNonExistingGroupCheckout_WhenInitiate_ThenSucceeds()
    {
        var guestStaysIds = new[] { Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7() };

        Spec.Given()
            .When(_ => GroupCheckout.Initiate(groupCheckoutId, clerkId, guestStaysIds, now))
            .Then(new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStaysIds, now));
    }
}
