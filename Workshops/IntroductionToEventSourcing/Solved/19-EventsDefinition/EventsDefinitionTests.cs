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
        var guestStayId = Guid.CreateVersion7();
        var groupCheckoutId = Guid.CreateVersion7();

        // When
        var events = new GuestStayAccountEvent[]
        {
            new GuestCheckedIn(guestStayId, DateTimeOffset.Now),
            new ChargeRecorded(guestStayId, 100, DateTimeOffset.Now),
            new PaymentRecorded(guestStayId, 100, DateTimeOffset.Now),
            new GuestCheckedOut(guestStayId, DateTimeOffset.Now, groupCheckoutId),
            new GuestStayAccountEvent.GuestCheckOutFailed(guestStayId,
                GuestStayAccountEvent.GuestCheckOutFailed.FailureReason.NotOpened, DateTimeOffset.Now,
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
        var groupCheckoutId = Guid.CreateVersion7();
        Guid[] guestStayIds = [Guid.CreateVersion7(), Guid.CreateVersion7()];
        var clerkId = Guid.CreateVersion7();

        // When
        var events = new GroupCheckoutEvent[]
        {
            new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStayIds, DateTimeOffset.Now),
            new GuestCheckoutCompleted(groupCheckoutId, guestStayIds[0], DateTimeOffset.Now),
            new GroupCheckoutEvent.GuestCheckOutFailed(groupCheckoutId, guestStayIds[1], DateTimeOffset.Now),
            new GroupCheckoutFailed(groupCheckoutId, [guestStayIds[0]], [guestStayIds[1]], DateTimeOffset.Now),
            new GroupCheckoutCompleted(groupCheckoutId, guestStayIds, DateTimeOffset.Now)
        };

        // Then
        const int minimumExpectedEventTypesCount = 5;
        events.Should().HaveCountGreaterOrEqualTo(minimumExpectedEventTypesCount);
        events.GroupBy(e => e.GetType()).Should().HaveCountGreaterOrEqualTo(minimumExpectedEventTypesCount);
    }
}
