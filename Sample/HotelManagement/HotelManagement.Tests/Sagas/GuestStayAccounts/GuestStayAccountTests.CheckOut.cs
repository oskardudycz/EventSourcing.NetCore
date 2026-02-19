using HotelManagement.Sagas.GuestStayAccounts;
using Ogooreck.BusinessLogic;
using Xunit;

namespace HotelManagement.Tests.Sagas.GuestStayAccounts;

using static GuestCheckoutFailed;

public partial class GuestStayAccountTests
{
    private readonly Guid groupCheckoutId = Guid.CreateVersion7();

    [Fact]
    public void GivenNonExistingGuestStayAccount_WhenCheckOut_ThenFails() =>
        Spec.Given()
            .When(state => state.CheckOut(now, groupCheckoutId).FlatMap())
            .Then(new GuestCheckoutFailed(Guid.Empty, FailureReason.NotOpened, now, groupCheckoutId));

    [Fact]
    public void GivenCheckedInGuestStayAccount_WhenCheckOut_ThenSucceeds() =>
        Spec.Given(new GuestCheckedIn(guestStayAccountId, faker.Date.RecentOffset()))
            .When(state => state.CheckOut(now, groupCheckoutId).FlatMap())
            .Then(new GuestCheckedOut(guestStayAccountId, now, groupCheckoutId));

    [Fact]
    public void GivenUnsettledCheckedInGuestStayAccountWithCharge_WhenCheckOut_ThenFailsWithEvent()
    {
        var checkedInDate = faker.Date.RecentOffset();

        Spec.Given(
                new GuestCheckedIn(guestStayAccountId, faker.Date.RecentOffset()),
                new ChargeRecorded(guestStayAccountId, faker.Random.Decimal(0.1M),
                    faker.Date.RecentOffset(refDate: checkedInDate))
            )
            .When(state => state.CheckOut(now, groupCheckoutId).FlatMap())
            .Then(new GuestCheckoutFailed(guestStayAccountId, FailureReason.BalanceNotSettled, now, groupCheckoutId));
    }

    [Fact]
    public void GivenUnsettledCheckedInGuestStayAccountWithPayment_WhenCheckOut_ThenFailsWithEvent()
    {
        var checkedInDate = faker.Date.RecentOffset();

        Spec.Given(
                new GuestCheckedIn(guestStayAccountId, faker.Date.RecentOffset()),
                new PaymentRecorded(guestStayAccountId, faker.Random.Decimal(0.1M),
                    faker.Date.RecentOffset(refDate: checkedInDate))
            )
            .When(state => state.CheckOut(now, groupCheckoutId).FlatMap())
            .Then(new GuestCheckoutFailed(guestStayAccountId, FailureReason.BalanceNotSettled, now, groupCheckoutId));
    }

    [Fact]
    public void GivenCheckedInWithSettledChargesGuestStayAccount_WhenCheckOut_ThenSucceeds()
    {
        var settledAmount = faker.Random.Decimal(0.1M);
        var checkedInDate = faker.Date.RecentOffset();

        Spec.Given(
                new GuestCheckedIn(guestStayAccountId, faker.Date.RecentOffset()),
                new ChargeRecorded(guestStayAccountId, settledAmount, faker.Date.RecentOffset(refDate: checkedInDate)),
                new PaymentRecorded(guestStayAccountId, settledAmount, faker.Date.RecentOffset(refDate: checkedInDate))
            )
            .When(state => state.CheckOut(now, groupCheckoutId).FlatMap())
            .Then(new GuestCheckedOut(guestStayAccountId, now, groupCheckoutId));
    }

    [Fact]
    public void GivenCheckedInWithNotSettledChargesGuestStayAccount_WhenCheckOut_ThenFails()
    {
        var chargeAmount = faker.Random.Decimal(0.1M);
        var checkedInDate = faker.Date.RecentOffset();

        Spec.Given(
                new GuestCheckedIn(guestStayAccountId, faker.Date.RecentOffset()),
                new ChargeRecorded(guestStayAccountId, chargeAmount, faker.Date.RecentOffset(refDate: checkedInDate)),
                new PaymentRecorded(guestStayAccountId, faker.Random.Decimal(chargeAmount),
                    faker.Date.RecentOffset(refDate: checkedInDate))
            )
            .When(state => state.CheckOut(now, groupCheckoutId).FlatMap())
            .Then(new GuestCheckoutFailed(guestStayAccountId, FailureReason.BalanceNotSettled, now, groupCheckoutId));
    }

    [Fact]
    public void GivenCheckedOutGuestStayAccount_WhenCheckOut_ThenSucceeds()
    {
        var checkedInDate = faker.Date.RecentOffset();

        Spec.Given(
                new GuestCheckedIn(guestStayAccountId, faker.Date.RecentOffset()),
                new GuestCheckedOut(guestStayAccountId, faker.Date.RecentOffset(refDate: checkedInDate))
            )
            .When(state => state.CheckOut(now, groupCheckoutId).FlatMap())
            .ThenThrows<InvalidOperationException>();
    }

    [Fact]
    public void GivenCheckedOutWithSettledChargesGuestStayAccount_WhenCheckOut_ThenFails()
    {
        var settledAmount = faker.Random.Decimal(0.1M);
        var checkedInDate = faker.Date.RecentOffset();

        Spec.Given(
                new GuestCheckedIn(guestStayAccountId, faker.Date.RecentOffset()),
                new ChargeRecorded(guestStayAccountId, settledAmount, faker.Date.RecentOffset(refDate: checkedInDate)),
                new PaymentRecorded(guestStayAccountId, settledAmount, faker.Date.RecentOffset(refDate: checkedInDate)),
                new GuestCheckedOut(guestStayAccountId, faker.Date.RecentOffset(refDate: checkedInDate))
            )
            .When(state => state.CheckOut(now, groupCheckoutId).FlatMap())
            .Then(new GuestCheckoutFailed(guestStayAccountId, FailureReason.NotOpened, now, groupCheckoutId));
    }
}
