using FluentAssertions;
using IntroductionToEventSourcing.Projections.MultiStream.OutOfOrder.Tools;
using Xunit;

namespace IntroductionToEventSourcing.Projections.MultiStream.OutOfOrder;

// EVENTS
public record PaymentRecorded(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount
);

public record MerchantLimitsChecked(
    Guid PaymentId,
    Guid MerchantId,
    bool IsWithinLimits
);

public record FraudScoreCalculated(
    Guid PaymentId,
    decimal Score,
    bool IsAcceptable
);

// ENUMS
public enum VerificationStatus
{
    Pending,
    Passed,
    Failed
}

public enum PaymentStatus
{
    Pending,
    Approved,
    Rejected
}

public enum DataQuality
{
    Partial,
    Sufficient,
    Complete
}

// READ MODEL
public class PaymentVerification
{
    public Guid Id { get; set; }
    public PaymentStatus Status { get; set; }
}

public class ProjectionsTests
{
    [Fact]
    [Trait("Category", "SkipCI")]
    public void MultiStreamProjection_WithOutOfOrderEvents_ShouldSucceed()
    {
        var payment1Id = Guid.CreateVersion7();
        var payment2Id = Guid.CreateVersion7();
        var payment3Id = Guid.CreateVersion7();
        var payment4Id = Guid.CreateVersion7();

        var order1Id = Guid.CreateVersion7();
        var order2Id = Guid.CreateVersion7();
        var order3Id = Guid.CreateVersion7();
        var order4Id = Guid.CreateVersion7();

        var merchant1Id = Guid.CreateVersion7();
        var merchant2Id = Guid.CreateVersion7();

        var fraudCheck1Id = Guid.CreateVersion7();
        var fraudCheck2Id = Guid.CreateVersion7();
        var fraudCheck3Id = Guid.CreateVersion7();
        var fraudCheck4Id = Guid.CreateVersion7();

        var eventStore = new EventStore();
        var database = new Database();

        // TODO:
        // 1. Create a PaymentVerificationProjection class that handles each event type.
        // 2. Each handler must work even if events arrive out of order (e.g., fraud score before payment).
        // 3. The projection should derive the Status based on available data:
        //    - Pending: waiting for required data
        //    - Rejected: merchant limits failed OR fraud score unacceptable
        //    - Approved: all checks passed
        // 4. Register your event handlers using `eventStore.Register`.

        // Payment 1: Approved — events arrive out of order (fraud score first!)
        eventStore.Append(fraudCheck1Id, new FraudScoreCalculated(payment1Id, 0.1m, true));
        eventStore.Append(merchant1Id, new MerchantLimitsChecked(payment1Id, merchant1Id, true));
        eventStore.Append(payment1Id, new PaymentRecorded(payment1Id, order1Id, 100m));

        // Payment 2: Merchant rejected — merchant check arrives first
        eventStore.Append(merchant2Id, new MerchantLimitsChecked(payment2Id, merchant2Id, false));
        eventStore.Append(fraudCheck2Id, new FraudScoreCalculated(payment2Id, 0.2m, true));
        eventStore.Append(payment2Id, new PaymentRecorded(payment2Id, order2Id, 5000m));

        // Payment 3: Fraud rejected — payment recorded last
        eventStore.Append(fraudCheck3Id, new FraudScoreCalculated(payment3Id, 0.95m, false));
        eventStore.Append(merchant1Id, new MerchantLimitsChecked(payment3Id, merchant1Id, true));
        eventStore.Append(payment3Id, new PaymentRecorded(payment3Id, order3Id, 200m));

        // Payment 4: Pending — missing fraud check (payment recorded, merchant checked, but no fraud score yet)
        eventStore.Append(payment4Id, new PaymentRecorded(payment4Id, order4Id, 50m));
        eventStore.Append(merchant1Id, new MerchantLimitsChecked(payment4Id, merchant1Id, true));

        // Assert Payment 1: Approved (all data arrived, all checks passed)
        var payment1 = database.Get<PaymentVerification>(payment1Id)!;
        payment1.Should().NotBeNull();
        payment1.Id.Should().Be(payment1Id);
        payment1.Status.Should().Be(PaymentStatus.Approved);

        // Assert Payment 2: Merchant rejected
        var payment2 = database.Get<PaymentVerification>(payment2Id)!;
        payment2.Should().NotBeNull();
        payment2.Id.Should().Be(payment2Id);
        payment2.Status.Should().Be(PaymentStatus.Rejected);

        // Assert Payment 3: Fraud rejected
        var payment3 = database.Get<PaymentVerification>(payment3Id)!;
        payment3.Should().NotBeNull();
        payment3.Id.Should().Be(payment3Id);
        payment3.Status.Should().Be(PaymentStatus.Rejected);

        // Assert Payment 4: Pending (waiting for fraud check)
        var payment4 = database.Get<PaymentVerification>(payment4Id)!;
        payment4.Should().NotBeNull();
        payment4.Id.Should().Be(payment4Id);
        payment4.Status.Should().Be(PaymentStatus.Pending);
    }
}
