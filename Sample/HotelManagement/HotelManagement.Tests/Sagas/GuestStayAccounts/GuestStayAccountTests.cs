using Bogus;
using HotelManagement.Sagas.GuestStayAccounts;
using Ogooreck.BusinessLogic;

namespace HotelManagement.Tests.Sagas.GuestStayAccounts;

public partial class GuestStayAccountTests
{
    private readonly HandlerSpecification<GuestStayAccount> Spec = Specification.For<GuestStayAccount>(Evolve);
    private readonly DateTimeOffset now = DateTimeOffset.UtcNow;
    private readonly Guid guestStayAccountId = Guid.NewGuid();
    private readonly Faker faker = new();

    private static GuestStayAccount Evolve(GuestStayAccount guestStayAccount, object @event)
    {
        return @event switch
        {
            GuestCheckedIn guestCheckedIn => GuestStayAccount.Create(guestCheckedIn),
            ChargeRecorded chargeRecorded => guestStayAccount.Apply(chargeRecorded),
            PaymentRecorded paymentRecorded => guestStayAccount.Apply(paymentRecorded),
            GuestCheckedOut guestCheckedOut => guestStayAccount.Apply(guestCheckedOut),
            _ => guestStayAccount
        };
    }
}
