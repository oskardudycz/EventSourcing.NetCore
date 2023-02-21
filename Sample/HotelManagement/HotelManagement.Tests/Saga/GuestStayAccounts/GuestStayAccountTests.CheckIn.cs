using HotelManagement.Saga.GuestStayAccounts;
using Ogooreck.BusinessLogic;
using Xunit;

namespace HotelManagement.Tests.Saga.GuestStayAccounts;

public partial class GuestStayAccountTests
{
    [Fact]
    public void GivenNonExistingGuestStayAccount_WhenCheckIn_ThenSucceeds() =>
        Spec.Given()
            .When(_ => GuestStayAccount.CheckIn(guestStayAccountId, now))
            .Then(new GuestCheckedIn(guestStayAccountId, now));
}
