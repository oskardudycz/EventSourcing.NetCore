using Bogus;
using HotelManagement.GroupCheckouts;
using Ogooreck.BusinessLogic;

namespace HotelManagement.Tests.GroupCheckouts;

public partial class GroupCheckoutTests
{
    private readonly HandlerSpecification<GroupCheckout> Spec = Specification.For<GroupCheckout>(Evolve);
    private readonly DateTimeOffset now = DateTimeOffset.UtcNow;
    private readonly Guid groupCheckoutId = Guid.NewGuid();
    private readonly Guid clerkId = Guid.NewGuid();
    private readonly Faker faker = new();

    private static GroupCheckout Evolve(GroupCheckout groupCheckout, object @event)
    {
        return @event switch
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
}
