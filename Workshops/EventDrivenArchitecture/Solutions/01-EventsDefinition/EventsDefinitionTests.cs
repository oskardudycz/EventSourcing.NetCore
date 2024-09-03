using FluentAssertions;
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
        var guestStayId = Guid.NewGuid();
        var groupCheckoutId = Guid.NewGuid();

        // When
        var events = new GuestStayAccountEvent[]
        {
            new GuestCheckedIn(guestStayId, DateTimeOffset.Now),
            new ChargeRecorded(guestStayId, 100, DateTimeOffset.Now),
            new PaymentRecorded(guestStayId, 100, DateTimeOffset.Now),
            new GuestCheckedOut(guestStayId, DateTimeOffset.Now, groupCheckoutId),
            new GuestStayAccountEvent.GuestCheckoutFailed(guestStayId,
                GuestStayAccountEvent.GuestCheckoutFailed.FailureReason.NotOpened, DateTimeOffset.Now,
                groupCheckoutId)
        };

        // Then
        const int minimumExpectedEventTypesCount = 5;
        events.Should().HaveCountGreaterOrEqualTo(minimumExpectedEventTypesCount);
        events.GroupBy(e => e.GetType()).Should().HaveCountGreaterOrEqualTo(minimumExpectedEventTypesCount);
    }

    [Fact]
    public void GroupCheckoutEventTypes_AreDefined()
    {
        // Given
        var groupCheckoutId = Guid.NewGuid();
        Guid[] guestStayIds = [Guid.NewGuid(), Guid.NewGuid()];
        var clerkId = Guid.NewGuid();

        // When
        var events = new GroupCheckoutEvent[]
        {
            new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStayIds, DateTimeOffset.Now),
            new GuestCheckoutCompleted(groupCheckoutId, guestStayIds[0], DateTimeOffset.Now),
            new GroupCheckoutEvent.GuestCheckoutFailed(groupCheckoutId, guestStayIds[1], DateTimeOffset.Now),
            new GroupCheckoutFailed(groupCheckoutId, [guestStayIds[0]], [guestStayIds[1]], DateTimeOffset.Now),
            new GroupCheckoutCompleted(groupCheckoutId, guestStayIds, DateTimeOffset.Now)
        };

        // Then
        const int minimumExpectedEventTypesCount = 5;
        events.Should().HaveCountGreaterOrEqualTo(minimumExpectedEventTypesCount);
        events.GroupBy(e => e.GetType()).Should().HaveCountGreaterOrEqualTo(minimumExpectedEventTypesCount);
    }
}
