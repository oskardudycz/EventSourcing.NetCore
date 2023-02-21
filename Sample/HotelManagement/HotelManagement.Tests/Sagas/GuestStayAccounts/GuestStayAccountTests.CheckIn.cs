using HotelManagement.Sagas.GuestStayAccounts;
using Ogooreck.BusinessLogic;
using Xunit;

namespace HotelManagement.Tests.Sagas.GuestStayAccounts;

public partial class GuestStayAccountTests
{
    [Fact]
    public void GivenNonExistingGuestStayAccount_WhenCheckIn_ThenSucceeds() =>
        Spec.Given()
            .When(_ => GuestStayAccount.CheckIn(guestStayAccountId, now))
            .Then(new GuestCheckedIn(guestStayAccountId, now));
}
