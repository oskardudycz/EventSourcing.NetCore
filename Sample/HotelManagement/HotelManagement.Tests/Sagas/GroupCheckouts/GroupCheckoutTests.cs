using Bogus;
using HotelManagement.Sagas.GroupCheckouts;
using Ogooreck.BusinessLogic;

namespace HotelManagement.Tests.Sagas.GroupCheckouts;

public partial class GroupCheckoutTests
{
    private readonly HandlerSpecification<GroupCheckout> Spec = Specification.For<GroupCheckout>(Evolve);
    private readonly DateTimeOffset now = DateTimeOffset.UtcNow;
    private readonly Guid groupCheckoutId = Guid.CreateVersion7();
    private readonly Guid clerkId = Guid.CreateVersion7();
    private readonly Faker faker = new();

    private static GroupCheckout Evolve(GroupCheckout groupCheckout, object @event) =>
        @event switch
        {
            GroupCheckoutInitiated groupCheckoutInitiated => GroupCheckout.Create(groupCheckoutInitiated),
            GuestCheckoutsInitiated guestCheckoutsInitiated => groupCheckout.Apply(guestCheckoutsInitiated),
            GuestCheckoutCompleted guestCheckoutCompleted => groupCheckout.Apply(guestCheckoutCompleted),
            GuestCheckoutFailed guestCheckoutFailed => groupCheckout.Apply(guestCheckoutFailed),
            GroupCheckoutCompleted groupCheckoutCompleted => groupCheckout.Apply(groupCheckoutCompleted),
            GroupCheckoutFailed groupCheckoutFailed => groupCheckout.Apply(groupCheckoutFailed),
            _ => groupCheckout
        };
}
