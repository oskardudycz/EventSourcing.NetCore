using FluentAssertions;
using IntroductionToEventSourcing.Projections.MultiStream.Tools;
using Xunit;

namespace IntroductionToEventSourcing.Projections.MultiStream;

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
    public void MultiStreamProjection_ForPaymentVerification_ShouldSucceed()
    {
        var payment1Id = Guid.CreateVersion7();
        var payment2Id = Guid.CreateVersion7();
        var payment3Id = Guid.CreateVersion7();
        var payment4Id = Guid.CreateVersion7();
        var payment5Id = Guid.CreateVersion7();

        var order1Id = Guid.CreateVersion7();
        var order2Id = Guid.CreateVersion7();
        var order3Id = Guid.CreateVersion7();
        var order4Id = Guid.CreateVersion7();
        var order5Id = Guid.CreateVersion7();

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
        //    Events arrive on different streams (payment, merchant, fraud check),
        //    but they share PaymentId — use PaymentId as the read model key.
        // 2. Register your event handlers using `eventStore.Register`.

        // Payment 1: Approved — all checks pass
        eventStore.Append(payment1Id, new PaymentRecorded(payment1Id, order1Id, 100m));
        eventStore.Append(merchant1Id, new MerchantLimitsChecked(payment1Id, merchant1Id, true));
        eventStore.Append(fraudCheck1Id, new FraudScoreCalculated(payment1Id, 0.1m, true));

        // Payment 2: Rejected — merchant limits failed
        eventStore.Append(payment2Id, new PaymentRecorded(payment2Id, order2Id, 5000m));
        eventStore.Append(merchant2Id, new MerchantLimitsChecked(payment2Id, merchant2Id, false));
        eventStore.Append(fraudCheck2Id, new FraudScoreCalculated(payment2Id, 0.2m, true));

        // Payment 3: Rejected — high fraud score
        eventStore.Append(payment3Id, new PaymentRecorded(payment3Id, order3Id, 200m));
        eventStore.Append(merchant1Id, new MerchantLimitsChecked(payment3Id, merchant1Id, true));
        eventStore.Append(fraudCheck3Id, new FraudScoreCalculated(payment3Id, 0.95m, false));

        // Payment 4: Rejected — large amount + elevated fraud risk
        eventStore.Append(payment4Id, new PaymentRecorded(payment4Id, order4Id, 15000m));
        eventStore.Append(merchant1Id, new MerchantLimitsChecked(payment4Id, merchant1Id, true));
        eventStore.Append(fraudCheck4Id, new FraudScoreCalculated(payment4Id, 0.6m, true));

        // Payment 5: Pending — missing fraud check
        eventStore.Append(payment5Id, new PaymentRecorded(payment5Id, order5Id, 50m));
        eventStore.Append(merchant1Id, new MerchantLimitsChecked(payment5Id, merchant1Id, true));

        // Assert Payment 1: Approved
        var payment1 = database.Get<PaymentVerification>(payment1Id)!;
        payment1.Should().NotBeNull();
        payment1.Status.Should().Be(PaymentStatus.Approved);

        // Assert Payment 2: Rejected
        var payment2 = database.Get<PaymentVerification>(payment2Id)!;
        payment2.Should().NotBeNull();
        payment2.Status.Should().Be(PaymentStatus.Rejected);

        // Assert Payment 3: Rejected
        var payment3 = database.Get<PaymentVerification>(payment3Id)!;
        payment3.Should().NotBeNull();
        payment3.Status.Should().Be(PaymentStatus.Rejected);

        // Assert Payment 4: Rejected
        var payment4 = database.Get<PaymentVerification>(payment4Id)!;
        payment4.Should().NotBeNull();
        payment4.Status.Should().Be(PaymentStatus.Rejected);

        // Assert Payment 5: Pending
        var payment5 = database.Get<PaymentVerification>(payment5Id)!;
        payment5.Should().NotBeNull();
        payment5.Status.Should().Be(PaymentStatus.Pending);
    }
}
