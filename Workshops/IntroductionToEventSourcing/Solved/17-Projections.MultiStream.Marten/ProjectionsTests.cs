using Shouldly;
using JasperFx;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Weasel.Core;
using Xunit;
using IntroductionToEventSourcing.Tools;

namespace IntroductionToEventSourcing.Projections.MultiStream.Marten;

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
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public VerificationStatus MerchantLimitStatus { get; set; }
    public VerificationStatus FraudStatus { get; set; }
    public decimal FraudScore { get; set; }
    public PaymentStatus Status { get; set; }
}

public class PaymentVerificationProjection: MultiStreamProjection<PaymentVerification, Guid>
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

        if (item.MerchantLimitStatus == VerificationStatus.Failed || item.FraudScore > 0.75m || item is { Amount: > 10000m, FraudScore: > 0.5m })
            item.Status = PaymentStatus.Rejected;
        else
            item.Status = PaymentStatus.Approved;
    }
}

public class ProjectionsTests
{
    private static string ConnectionString => TestDatabases.PostgresConnectionString;

    [Fact]
    [Trait("Category", "SkipCI")]
    public async Task MultiStreamProjection_WithMarten_ShouldSucceed()
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

        await using var documentStore = DocumentStore.For(options =>
        {
            options.Connection(ConnectionString);
            options.DatabaseSchemaName = options.Events.DatabaseSchemaName = "Exercise17MultiStreamMarten";
            options.AutoCreateSchemaObjects = AutoCreate.All;

            options.Projections.Add<PaymentVerificationProjection>(ProjectionLifecycle.Async);
        });

        // Let's start Async Daemon to process async projections in the background
        // Read more: https://martendb.io/events/projections/async-daemon.html#async-projections-daemon
        using var daemon = await documentStore.BuildProjectionDaemonAsync();
        await daemon.StartAllAsync();

        await using var session = documentStore.LightweightSession();

        // Payment 1: Approved — all checks pass
        session.Events.Append(payment1Id, new PaymentRecorded(payment1Id, order1Id, 100m));
        session.Events.Append(merchant1Id, new MerchantLimitsChecked(payment1Id, merchant1Id, true));
        session.Events.Append(fraudCheck1Id, new FraudScoreCalculated(payment1Id, 0.1m, true));

        // Payment 2: Rejected — merchant limits failed
        session.Events.Append(payment2Id, new PaymentRecorded(payment2Id, order2Id, 5000m));
        session.Events.Append(merchant2Id, new MerchantLimitsChecked(payment2Id, merchant2Id, false));
        session.Events.Append(fraudCheck2Id, new FraudScoreCalculated(payment2Id, 0.2m, true));

        // Payment 3: Rejected — high fraud score
        session.Events.Append(payment3Id, new PaymentRecorded(payment3Id, order3Id, 200m));
        session.Events.Append(merchant1Id, new MerchantLimitsChecked(payment3Id, merchant1Id, true));
        session.Events.Append(fraudCheck3Id, new FraudScoreCalculated(payment3Id, 0.95m, false));

        // Payment 4: Rejected — large amount + elevated fraud risk
        session.Events.Append(payment4Id, new PaymentRecorded(payment4Id, order4Id, 15000m));
        session.Events.Append(merchant1Id, new MerchantLimitsChecked(payment4Id, merchant1Id, true));
        session.Events.Append(fraudCheck4Id, new FraudScoreCalculated(payment4Id, 0.6m, true));

        // Payment 5: Pending — missing fraud check
        session.Events.Append(payment5Id, new PaymentRecorded(payment5Id, order5Id, 50m));
        session.Events.Append(merchant1Id, new MerchantLimitsChecked(payment5Id, merchant1Id, true));

        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Wait until Async Daemon processes all events
        await daemon.WaitForNonStaleData(TimeSpan.FromSeconds(5));

        // Assert Payment 1: Approved
        var payment1 = await session.LoadAsync<PaymentVerification>(payment1Id, TestContext.Current.CancellationToken);
        payment1.ShouldNotBeNull();
        payment1!.Id.ShouldBe(payment1Id);
        payment1.OrderId.ShouldBe(order1Id);
        payment1.Amount.ShouldBe(100m);
        payment1.MerchantLimitStatus.ShouldBe(VerificationStatus.Passed);
        payment1.FraudStatus.ShouldBe(VerificationStatus.Passed);
        payment1.FraudScore.ShouldBe(0.1m);
        payment1.Status.ShouldBe(PaymentStatus.Approved);

        // Assert Payment 2: Rejected
        var payment2 = await session.LoadAsync<PaymentVerification>(payment2Id, TestContext.Current.CancellationToken);
        payment2.ShouldNotBeNull();
        payment2!.Id.ShouldBe(payment2Id);
        payment2.OrderId.ShouldBe(order2Id);
        payment2.Amount.ShouldBe(5000m);
        payment2.MerchantLimitStatus.ShouldBe(VerificationStatus.Failed);
        payment2.FraudStatus.ShouldBe(VerificationStatus.Passed);
        payment2.FraudScore.ShouldBe(0.2m);
        payment2.Status.ShouldBe(PaymentStatus.Rejected);

        // Assert Payment 3: Rejected
        var payment3 = await session.LoadAsync<PaymentVerification>(payment3Id, TestContext.Current.CancellationToken);
        payment3.ShouldNotBeNull();
        payment3!.Id.ShouldBe(payment3Id);
        payment3.OrderId.ShouldBe(order3Id);
        payment3.Amount.ShouldBe(200m);
        payment3.MerchantLimitStatus.ShouldBe(VerificationStatus.Passed);
        payment3.FraudStatus.ShouldBe(VerificationStatus.Failed);
        payment3.FraudScore.ShouldBe(0.95m);
        payment3.Status.ShouldBe(PaymentStatus.Rejected);

        // Assert Payment 4: Rejected
        var payment4 = await session.LoadAsync<PaymentVerification>(payment4Id, TestContext.Current.CancellationToken);
        payment4.ShouldNotBeNull();
        payment4!.Id.ShouldBe(payment4Id);
        payment4.OrderId.ShouldBe(order4Id);
        payment4.Amount.ShouldBe(15000m);
        payment4.MerchantLimitStatus.ShouldBe(VerificationStatus.Passed);
        payment4.FraudStatus.ShouldBe(VerificationStatus.Passed);
        payment4.FraudScore.ShouldBe(0.6m);
        payment4.Status.ShouldBe(PaymentStatus.Rejected);

        // Assert Payment 5: Pending
        var payment5 = await session.LoadAsync<PaymentVerification>(payment5Id, TestContext.Current.CancellationToken);
        payment5.ShouldNotBeNull();
        payment5!.Id.ShouldBe(payment5Id);
        payment5.OrderId.ShouldBe(order5Id);
        payment5.Amount.ShouldBe(50m);
        payment5.MerchantLimitStatus.ShouldBe(VerificationStatus.Passed);
        payment5.FraudStatus.ShouldBe(VerificationStatus.Pending);
        payment5.FraudScore.ShouldBe(0m);
        payment5.Status.ShouldBe(PaymentStatus.Pending);
    }
}
