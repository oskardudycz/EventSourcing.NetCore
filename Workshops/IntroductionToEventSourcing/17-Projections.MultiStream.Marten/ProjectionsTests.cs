using FluentAssertions;
using JasperFx;
using Marten;
using Marten.Events.Projections;
using Weasel.Core;
using Xunit;

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
    public PaymentStatus Status { get; set; }
}

public class ProjectionsTests
{
    private const string ConnectionString =
        "PORT = 5432; HOST = localhost; TIMEOUT = 15; POOLING = True; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'";

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

            // TODO:
            // 1. Create a PaymentVerificationProjection class that inherits from MultiStreamProjection<PaymentVerification, Guid>
            // 2. Register the projection here using: options.Projections.Add<PaymentVerificationProjection>(ProjectionLifecycle.Inline);
        });

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

        await session.SaveChangesAsync();

        // Assert Payment 1: Approved
        var payment1 = await session.LoadAsync<PaymentVerification>(payment1Id);
        payment1.Should().NotBeNull();
        payment1!.Status.Should().Be(PaymentStatus.Approved);

        // Assert Payment 2: Rejected
        var payment2 = await session.LoadAsync<PaymentVerification>(payment2Id);
        payment2.Should().NotBeNull();
        payment2!.Status.Should().Be(PaymentStatus.Rejected);

        // Assert Payment 3: Rejected
        var payment3 = await session.LoadAsync<PaymentVerification>(payment3Id);
        payment3.Should().NotBeNull();
        payment3!.Status.Should().Be(PaymentStatus.Rejected);

        // Assert Payment 4: Rejected
        var payment4 = await session.LoadAsync<PaymentVerification>(payment4Id);
        payment4.Should().NotBeNull();
        payment4!.Status.Should().Be(PaymentStatus.Rejected);

        // Assert Payment 5: Pending
        var payment5 = await session.LoadAsync<PaymentVerification>(payment5Id);
        payment5.Should().NotBeNull();
        payment5!.Status.Should().Be(PaymentStatus.Pending);
    }
}
