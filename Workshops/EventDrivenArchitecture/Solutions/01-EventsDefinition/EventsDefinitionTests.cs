using Shouldly;
using Xunit;

namespace EventsDefinition;

using static GuestStayAccountEvent;
using static GroupCheckoutEvent;

public class EventsDefinitionTests
{
    [Fact]
    public void GuestStayAccountEventTypes_AreDefined()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        var groupCheckoutId = Guid.NewGuid();

        // When
        var events = new GuestStayAccountEvent[]
        {
            new GuestCheckedIn(guestStayAccountId, DateTimeOffset.Now),
            new ChargeRecorded(guestStayAccountId, 100, DateTimeOffset.Now),
            new PaymentRecorded(guestStayAccountId, 100, DateTimeOffset.Now),
            new GuestCheckedOut(guestStayAccountId, DateTimeOffset.Now, groupCheckoutId),
            new GuestStayAccountEvent.GuestCheckoutFailed(guestStayAccountId,
                GuestStayAccountEvent.GuestCheckoutFailed.FailureReason.InvalidState, DateTimeOffset.Now,
                groupCheckoutId)
        };

        // Then
        const int expectedEventTypesCount = 5;
        events.Count().ShouldBe(expectedEventTypesCount);
        events.GroupBy(e => e.GetType()).Count().ShouldBe(expectedEventTypesCount);
    }

    [Fact]
    public void GroupCheckoutEventTypes_AreDefined()
    {
        // Given
        var groupCheckoutId = Guid.NewGuid();
        Guid[] guestStayAccountIds = [Guid.NewGuid(), Guid.NewGuid()];
        var clerkId = Guid.NewGuid();

        // When
        var events = new GroupCheckoutEvent[]
        {
            new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStayAccountIds, DateTimeOffset.Now),
            new GuestCheckoutCompletionRecorded(groupCheckoutId, guestStayAccountIds[0], DateTimeOffset.Now),
            new GroupCheckoutEvent.GuestCheckoutFailureRecorded(groupCheckoutId, guestStayAccountIds[1], DateTimeOffset.Now),
            new GroupCheckoutFailed(groupCheckoutId, [guestStayAccountIds[0]], [guestStayAccountIds[1]], DateTimeOffset.Now),
            new GroupCheckoutCompleted(groupCheckoutId, guestStayAccountIds, DateTimeOffset.Now)
        };

        // Then
        const int expectedEventTypesCount = 5;
        events.Count().ShouldBe(expectedEventTypesCount);
        events.GroupBy(e => e.GetType()).Count().ShouldBe(expectedEventTypesCount);
    }
}
