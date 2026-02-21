using FluentAssertions;
using JasperFx;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Xunit;

namespace IntroductionToEventSourcing.Projections.MultiStream.OutOfOrder.Marten;

// EVENTS
public record PaymentRecorded(
    string PaymentId,
    string OrderId,
    decimal Amount
);

public record MerchantLimitsChecked(
    string PaymentId,
    string MerchantId,
    bool IsWithinLimits
);

public record FraudScoreCalculated(
    string PaymentId,
    decimal Score,
    bool IsAcceptable
);

public record PaymentVerificationCompleted(
    string PaymentId,
    bool IsApproved
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
    public required string Id { get; set; }
    public required string OrderId { get; set; }
    public decimal Amount { get; set; }
    public VerificationStatus MerchantLimitStatus { get; set; }
    public VerificationStatus FraudStatus { get; set; }
    public decimal FraudScore { get; set; }
    public PaymentStatus Status { get; set; }
}

public class PaymentVerificationProjection: MultiStreamProjection<PaymentVerification, string>
{
    public PaymentVerificationProjection()
    {
        Identity<PaymentRecorded>(e => e.PaymentId);
        Identity<MerchantLimitsChecked>(e => e.PaymentId);
        Identity<FraudScoreCalculated>(e => e.PaymentId);
    }

    public void Apply(PaymentVerification item, PaymentRecorded @event)
    {
        item.Id = @event.PaymentId;
        item.OrderId = @event.OrderId;
        item.Amount = @event.Amount;
    }

    public void Apply(PaymentVerification item, MerchantLimitsChecked @event)
    {
        item.MerchantLimitStatus = @event.IsWithinLimits
            ? VerificationStatus.Passed
            : VerificationStatus.Failed;
    }

    public void Apply(PaymentVerification item, FraudScoreCalculated @event)
    {
        item.FraudScore = @event.Score;
        item.FraudStatus = @event.IsAcceptable
            ? VerificationStatus.Passed
            : VerificationStatus.Failed;

        if (item.Status != PaymentStatus.Pending)
            return;

        if (item.MerchantLimitStatus == VerificationStatus.Failed || item.FraudScore > 0.75m ||
            item is { Amount: > 10000m, FraudScore: > 0.5m })
            item.Status = PaymentStatus.Rejected;
        else
            item.Status = PaymentStatus.Approved;
    }
}

public class ProjectionsTests
{
    private const string ConnectionString =
        "PORT = 5432; HOST = localhost; TIMEOUT = 15; POOLING = True; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'";

    [Fact]
    [Trait("Category", "SkipCI")]
    public async Task MultiStreamProjection_WithOutOfOrderEventsAndMarten_ShouldSucceed()
    {
        var payment1Id = $"payment:{Guid.CreateVersion7()}";
        var payment2Id = $"payment:{Guid.CreateVersion7()}";
        var payment3Id = $"payment:{Guid.CreateVersion7()}";
        var payment4Id = $"payment:{Guid.CreateVersion7()}";
        var payment5Id = $"payment:{Guid.CreateVersion7()}";

        var order1Id = $"order:{Guid.CreateVersion7()}";
        var order2Id = $"order:{Guid.CreateVersion7()}";
        var order3Id = $"order:{Guid.CreateVersion7()}";
        var order4Id = $"order:{Guid.CreateVersion7()}";
        var order5Id = $"order:{Guid.CreateVersion7()}";

        var merchant1Id = $"merchant:{Guid.CreateVersion7()}";
        var merchant2Id = $"merchant:{Guid.CreateVersion7()}";

        var fraudCheck1Id = $"fraudcheck:{Guid.CreateVersion7()}";
        var fraudCheck2Id = $"fraudcheck:{Guid.CreateVersion7()}";
        var fraudCheck3Id = $"fraudcheck:{Guid.CreateVersion7()}";
        var fraudCheck4Id = $"fraudcheck:{Guid.CreateVersion7()}";

        await using var documentStore = DocumentStore.For(options =>
        {
            options.Connection(ConnectionString);
            options.DatabaseSchemaName = options.Events.DatabaseSchemaName = "Exercise18MultiStreamOutOfOrderMarten";
            options.AutoCreateSchemaObjects = AutoCreate.All;

            // TODO: This projection was built assuming ordered events. Run the test — it fails.
            // Events can arrive out of order (e.g. from different RabbitMQ queues or Kafka topics).
            // Fix it to handle out-of-order events and derive the verification decision.

            options.Projections.Add<PaymentVerificationProjection>(ProjectionLifecycle.Inline);
            options.Events.StreamIdentity = StreamIdentity.AsString;
        });

        await using var session = documentStore.LightweightSession();

        // Payment 1: Approved — FraudScore arrives first
        session.Events.Append(fraudCheck1Id, new FraudScoreCalculated(payment1Id, 0.1m, true));
        session.Events.Append(merchant1Id, new MerchantLimitsChecked(payment1Id, merchant1Id, true));
        session.Events.Append(payment1Id, new PaymentRecorded(payment1Id, order1Id, 100m));

        // Payment 2: Rejected — Merchant fails, arrives first
        session.Events.Append(merchant2Id, new MerchantLimitsChecked(payment2Id, merchant2Id, false));
        session.Events.Append(fraudCheck2Id, new FraudScoreCalculated(payment2Id, 0.2m, true));
        session.Events.Append(payment2Id, new PaymentRecorded(payment2Id, order2Id, 5000m));

        // Payment 3: Rejected — high fraud score arrives first
        session.Events.Append(fraudCheck3Id, new FraudScoreCalculated(payment3Id, 0.95m, false));
        session.Events.Append(merchant1Id, new MerchantLimitsChecked(payment3Id, merchant1Id, true));
        session.Events.Append(payment3Id, new PaymentRecorded(payment3Id, order3Id, 200m));

        // Payment 4: Rejected — fraud 0.6 looks OK until 15000 amount arrives last
        session.Events.Append(fraudCheck4Id, new FraudScoreCalculated(payment4Id, 0.6m, true));
        session.Events.Append(merchant1Id, new MerchantLimitsChecked(payment4Id, merchant1Id, true));
        session.Events.Append(payment4Id, new PaymentRecorded(payment4Id, order4Id, 15000m));

        // Payment 5: Pending — no fraud check
        session.Events.Append(merchant1Id, new MerchantLimitsChecked(payment5Id, merchant1Id, true));
        session.Events.Append(payment5Id, new PaymentRecorded(payment5Id, order5Id, 50m));

        await session.SaveChangesAsync();

        // Assert Payment 1: Approved
        var payment1 = await session.LoadAsync<PaymentVerification>(payment1Id);
        payment1.Should().NotBeNull();
        payment1.Status.Should().Be(PaymentStatus.Approved);

        // Assert Payment 2: Rejected
        var payment2 = await session.LoadAsync<PaymentVerification>(payment2Id);
        payment2.Should().NotBeNull();
        payment2.Status.Should().Be(PaymentStatus.Rejected);

        // Assert Payment 3: Rejected
        var payment3 = await session.LoadAsync<PaymentVerification>(payment3Id);
        payment3.Should().NotBeNull();
        payment3.Status.Should().Be(PaymentStatus.Rejected);

        // Assert Payment 4: Rejected
        var payment4 = await session.LoadAsync<PaymentVerification>(payment4Id);
        payment4.Should().NotBeNull();
        payment4.Status.Should().Be(PaymentStatus.Rejected);

        // Assert Payment 5: Pending
        var payment5 = await session.LoadAsync<PaymentVerification>(payment5Id);
        payment5.Should().NotBeNull();
        payment5.Status.Should().Be(PaymentStatus.Pending);

        // Assert Payment 1: Verification is emitted
        var paymentVerification = await session.Events.QueryRawEventDataOnly<PaymentVerificationCompleted>()
            .SingleOrDefaultAsync(e => e.PaymentId == payment1Id);
        paymentVerification.Should().NotBeNull();
        paymentVerification.PaymentId.Should().Be(payment1Id);
        paymentVerification.IsApproved.Should().BeTrue();
    }
}
