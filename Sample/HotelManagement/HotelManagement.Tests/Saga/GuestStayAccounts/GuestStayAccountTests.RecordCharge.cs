using HotelManagement.Saga.GuestStayAccounts;
using Ogooreck.BusinessLogic;
using Xunit;

namespace HotelManagement.Tests.Saga.GuestStayAccounts;

public partial class GuestStayAccountTests
{
    [Fact]
    public void GivenNonExistingGuestStayAccount_WhenRecordCharge_ThenFails() =>
        Spec.Given()
            .When(state => state.RecordCharge(faker.Random.Decimal(0.1M), now))
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void GivenCheckedInGuestStayAccount_WhenRecordCharge_ThenSucceeds()
    {
        var amount = faker.Random.Decimal(0.1M);

        Spec.Given(new GuestCheckedIn(guestStayAccountId, faker.Date.RecentOffset()))
            .When(state => state.RecordCharge(amount, now))
            .Then(new ChargeRecorded(guestStayAccountId, amount, now));
    }

    [Fact]
    public void GivenUnsettledCheckedInGuestStayAccountWithCharge_WhenRecordCharge_ThenSucceeds()
    {
        var amount = faker.Random.Decimal(0.1M);
        var checkedInDate = faker.Date.RecentOffset();

        Spec.Given(
                new GuestCheckedIn(guestStayAccountId, faker.Date.RecentOffset()),
                new ChargeRecorded(guestStayAccountId, faker.Random.Decimal(0.1M),
                    faker.Date.RecentOffset(refDate: checkedInDate))
            )
            .When(state => state.RecordCharge(amount, now))
            .Then(new ChargeRecorded(guestStayAccountId, amount, now));
    }

    [Fact]
    public void GivenUnsettledCheckedInGuestStayAccountWithPayment_WhenRecordCharge_ThenSucceeds()
    {
        var amount = faker.Random.Decimal(0.1M);
        var checkedInDate = faker.Date.RecentOffset();

        Spec.Given(
                new GuestCheckedIn(guestStayAccountId, faker.Date.RecentOffset()),
                new PaymentRecorded(guestStayAccountId, faker.Random.Decimal(0.1M),
                    faker.Date.RecentOffset(refDate: checkedInDate))
            )
            .When(state => state.RecordCharge(amount, now))
            .Then(new ChargeRecorded(guestStayAccountId, amount, now));
    }

    [Fact]
    public void GivenCheckedInWithSettledChargesGuestStayAccount_WhenRecordCharge_ThenFails()
    {
        var settledAmount = faker.Random.Decimal(0.1M);
        var checkedInDate = faker.Date.RecentOffset();

        Spec.Given(
                new GuestCheckedIn(guestStayAccountId, faker.Date.RecentOffset()),
                new ChargeRecorded(guestStayAccountId, settledAmount, faker.Date.RecentOffset(refDate: checkedInDate)),
                new PaymentRecorded(guestStayAccountId, settledAmount, faker.Date.RecentOffset(refDate: checkedInDate))
            )
            .When(state => state.RecordCharge(faker.Random.Decimal(0.1M), now))
            .ThenThrows<InvalidOperationException>();
    }

    [Fact]
    public void GivenCheckedInWithNotSettledChargesGuestStayAccount_WhenRecordCharge_ThenFails()
    {
        var chargeAmount = faker.Random.Decimal(0.1M);
        var checkedInDate = faker.Date.RecentOffset();

        Spec.Given(
                new GuestCheckedIn(guestStayAccountId, faker.Date.RecentOffset()),
                new ChargeRecorded(guestStayAccountId, chargeAmount, faker.Date.RecentOffset(refDate: checkedInDate)),
                new PaymentRecorded(guestStayAccountId, faker.Random.Decimal(chargeAmount), faker.Date.RecentOffset(refDate: checkedInDate))
            )
            .When(state => state.RecordCharge(faker.Random.Decimal(0.1M), now))
            .ThenThrows<InvalidOperationException>();
    }

    [Fact]
    public void GivenCheckedOutGuestStayAccount_WhenRecordCharge_ThenFails()
    {
        var checkedInDate = faker.Date.RecentOffset();

        Spec.Given(
                new GuestCheckedIn(guestStayAccountId, faker.Date.RecentOffset()),
                new GuestCheckedOut(guestStayAccountId, faker.Date.RecentOffset(refDate: checkedInDate))
            )
            .When(state => state.RecordCharge(faker.Random.Decimal(0.1M), now))
            .ThenThrows<InvalidOperationException>();
    }

    [Fact]
    public void GivenCheckedOutWithSettledChargesGuestStayAccount_WhenRecordCharge_ThenFails()
    {
        var settledAmount = faker.Random.Decimal(0.1M);
        var checkedInDate = faker.Date.RecentOffset();

        Spec.Given(
                new GuestCheckedIn(guestStayAccountId, faker.Date.RecentOffset()),
                new ChargeRecorded(guestStayAccountId, settledAmount, faker.Date.RecentOffset(refDate: checkedInDate)),
                new PaymentRecorded(guestStayAccountId, settledAmount, faker.Date.RecentOffset(refDate: checkedInDate)),
                new GuestCheckedOut(guestStayAccountId, faker.Date.RecentOffset(refDate: checkedInDate))
            )
            .When(state => state.RecordCharge(faker.Random.Decimal(0.1M), now))
            .ThenThrows<InvalidOperationException>();
    }
}
