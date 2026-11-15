using Shouldly;
using JasperFx;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Weasel.Core;
using Xunit;
using IntroductionToEventSourcing.Tools;

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

public enum DataQuality
{
    Partial,
    Sufficient,
    Complete
}

// READ MODEL
public class PaymentVerification
{
    public required string Id { get; set; }
    public string? OrderId { get; set; }
    public decimal? Amount { get; set; }
    public VerificationStatus MerchantLimitStatus { get; set; }
    public VerificationStatus FraudStatus { get; set; }
    public decimal FraudScore { get; set; }
    public PaymentStatus Status { get; set; }
    public DataQuality DataQuality { get; set; }

    public void Recalculate()
    {
        if (Status != PaymentStatus.Pending)
            return;

        var hasMerchantCheck = MerchantLimitStatus != VerificationStatus.Pending;
        var hasFraudCheck = FraudStatus != VerificationStatus.Pending;
        var hasPaymentData = OrderId != null && Amount.HasValue;

        if (hasPaymentData && hasMerchantCheck && hasFraudCheck)
            DataQuality = DataQuality.Complete;
        else if (hasMerchantCheck || hasFraudCheck)
            DataQuality = DataQuality.Sufficient;
        else
            DataQuality = DataQuality.Partial;

        if (!hasPaymentData || !hasMerchantCheck || !hasFraudCheck)
            return;

        if (MerchantLimitStatus == VerificationStatus.Failed || FraudScore > 0.75m ||
            Amount > 10000m && FraudScore > 0.5m)
            Status = PaymentStatus.Rejected;
        else
            Status = PaymentStatus.Approved;
    }
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

        item.Recalculate();
    }

    public void Apply(PaymentVerification item, MerchantLimitsChecked @event)
    {
        item.Id = @event.PaymentId;
        item.MerchantLimitStatus = @event.IsWithinLimits
            ? VerificationStatus.Passed
            : VerificationStatus.Failed;

        item.Recalculate();
    }

    public void Apply(PaymentVerification item, FraudScoreCalculated @event)
    {
        item.Id = @event.PaymentId;
        item.FraudScore = @event.Score;
        item.FraudStatus = @event.IsAcceptable
            ? VerificationStatus.Passed
            : VerificationStatus.Failed;

        item.Recalculate();
    }

    public override ValueTask RaiseSideEffects(
        IDocumentOperations operations,
        IEventSlice<PaymentVerification> slice)
    {
        var item = slice.Snapshot;
        if (item is { Status: not PaymentStatus.Pending })
        {
            slice.AppendEvent(
                $"verification:{item.Id}",
                new PaymentVerificationCompleted(item.Id, item.Status == PaymentStatus.Approved)
            );
        }

        return ValueTask.CompletedTask;
    }
}

public class ProjectionsTests
{
    private static string ConnectionString => TestDatabases.PostgresConnectionString;

    [Fact]
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

            options.Projections.Add<PaymentVerificationProjection>(ProjectionLifecycle.Async);
            options.Events.StreamIdentity = StreamIdentity.AsString;
        });

        // Let's start Async Daemon to process async projections in the background
        // Read more: https://martendb.io/events/projections/async-daemon.html#async-projections-daemon
        using var daemon = await documentStore.BuildProjectionDaemonAsync();
        await daemon.StartAllAsync();

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

        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        await daemon.WaitForNonStaleData(TimeSpan.FromSeconds(5));

        // Assert Payment 1: Approved
        var payment1 = await session.LoadAsync<PaymentVerification>(payment1Id, TestContext.Current.CancellationToken);
        payment1.ShouldNotBeNull();
        payment1.Id.ShouldBe(payment1Id);
        payment1.OrderId.ShouldBe(order1Id);
        payment1.Amount.ShouldBe(100m);
        payment1.MerchantLimitStatus.ShouldBe(VerificationStatus.Passed);
        payment1.FraudStatus.ShouldBe(VerificationStatus.Passed);
        payment1.FraudScore.ShouldBe(0.1m);
        payment1.Status.ShouldBe(PaymentStatus.Approved);
        payment1.DataQuality.ShouldBe(DataQuality.Complete);

        // Assert Payment 2: Rejected
        var payment2 = await session.LoadAsync<PaymentVerification>(payment2Id, TestContext.Current.CancellationToken);
        payment2.ShouldNotBeNull();
        payment2.Id.ShouldBe(payment2Id);
        payment2.OrderId.ShouldBe(order2Id);
        payment2.Amount.ShouldBe(5000m);
        payment2.MerchantLimitStatus.ShouldBe(VerificationStatus.Failed);
        payment2.FraudStatus.ShouldBe(VerificationStatus.Passed);
        payment2.FraudScore.ShouldBe(0.2m);
        payment2.Status.ShouldBe(PaymentStatus.Rejected);
        payment2.DataQuality.ShouldBe(DataQuality.Complete);

        // Assert Payment 3: Rejected
        var payment3 = await session.LoadAsync<PaymentVerification>(payment3Id, TestContext.Current.CancellationToken);
        payment3.ShouldNotBeNull();
        payment3.Id.ShouldBe(payment3Id);
        payment3.OrderId.ShouldBe(order3Id);
        payment3.Amount.ShouldBe(200m);
        payment3.MerchantLimitStatus.ShouldBe(VerificationStatus.Passed);
        payment3.FraudStatus.ShouldBe(VerificationStatus.Failed);
        payment3.FraudScore.ShouldBe(0.95m);
        payment3.Status.ShouldBe(PaymentStatus.Rejected);
        payment3.DataQuality.ShouldBe(DataQuality.Complete);

        // Assert Payment 4: Rejected
        var payment4 = await session.LoadAsync<PaymentVerification>(payment4Id, TestContext.Current.CancellationToken);
        payment4.ShouldNotBeNull();
        payment4.Id.ShouldBe(payment4Id);
        payment4.OrderId.ShouldBe(order4Id);
        payment4.Amount.ShouldBe(15000m);
        payment4.MerchantLimitStatus.ShouldBe(VerificationStatus.Passed);
        payment4.FraudStatus.ShouldBe(VerificationStatus.Passed);
        payment4.FraudScore.ShouldBe(0.6m);
        payment4.Status.ShouldBe(PaymentStatus.Rejected);
        payment4.DataQuality.ShouldBe(DataQuality.Complete);

        // Assert Payment 5: Pending
        var payment5 = await session.LoadAsync<PaymentVerification>(payment5Id, TestContext.Current.CancellationToken);
        payment5.ShouldNotBeNull();
        payment5.Id.ShouldBe(payment5Id);
        payment5.OrderId.ShouldBe(order5Id);
        payment5.Amount.ShouldBe(50m);
        payment5.MerchantLimitStatus.ShouldBe(VerificationStatus.Passed);
        payment5.FraudStatus.ShouldBe(VerificationStatus.Pending);
        payment5.FraudScore.ShouldBe(0m);
        payment5.Status.ShouldBe(PaymentStatus.Pending);
        payment5.DataQuality.ShouldBe(DataQuality.Sufficient);

        // Assert Payment 1: Verification is emitted
        var paymentVerification = await session.Events.QueryRawEventDataOnly<PaymentVerificationCompleted>()
            .SingleOrDefaultAsync(e => e.PaymentId == payment1Id, token: TestContext.Current.CancellationToken);
        paymentVerification.ShouldNotBeNull();
        paymentVerification.PaymentId.ShouldBe(payment1Id);
        paymentVerification.IsApproved.ShouldBeTrue();
    }
}
