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

public record PaymentVerificationCompleted(
    Guid PaymentId,
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

        var order1Id = Guid.CreateVersion7();
        var order2Id = Guid.CreateVersion7();
        var order3Id = Guid.CreateVersion7();
        var order4Id = Guid.CreateVersion7();

        var merchant1Id = Guid.CreateVersion7();
        var merchant2Id = Guid.CreateVersion7();

        var fraudCheck1Id = Guid.CreateVersion7();
        var fraudCheck2Id = Guid.CreateVersion7();
        var fraudCheck3Id = Guid.CreateVersion7();

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
        session.Events.Append(payment1Id, new PaymentVerificationCompleted(payment1Id, true));

        // Payment 2: Merchant rejected — exceeds merchant limits
        session.Events.Append(payment2Id, new PaymentRecorded(payment2Id, order2Id, 5000m));
        session.Events.Append(merchant2Id, new MerchantLimitsChecked(payment2Id, merchant2Id, false));
        session.Events.Append(fraudCheck2Id, new FraudScoreCalculated(payment2Id, 0.2m, true));
        session.Events.Append(payment2Id, new PaymentVerificationCompleted(payment2Id, false));

        // Payment 3: Fraud rejected — high fraud score
        session.Events.Append(payment3Id, new PaymentRecorded(payment3Id, order3Id, 200m));
        session.Events.Append(merchant1Id, new MerchantLimitsChecked(payment3Id, merchant1Id, true));
        session.Events.Append(fraudCheck3Id, new FraudScoreCalculated(payment3Id, 0.95m, false));
        session.Events.Append(payment3Id, new PaymentVerificationCompleted(payment3Id, false));

        // Payment 4: Pending — still awaiting fraud check and final decision
        session.Events.Append(payment4Id, new PaymentRecorded(payment4Id, order4Id, 50m));
        session.Events.Append(merchant1Id, new MerchantLimitsChecked(payment4Id, merchant1Id, true));

        await session.SaveChangesAsync();

        // Assert Payment 1: Approved
        var payment1 = await session.LoadAsync<PaymentVerification>(payment1Id);
        payment1.Should().NotBeNull();
        payment1!.Id.Should().Be(payment1Id);
        payment1.Status.Should().Be(PaymentStatus.Approved);

        // Assert Payment 2: Merchant rejected
        var payment2 = await session.LoadAsync<PaymentVerification>(payment2Id);
        payment2.Should().NotBeNull();
        payment2!.Id.Should().Be(payment2Id);
        payment2.Status.Should().Be(PaymentStatus.Rejected);

        // Assert Payment 3: Fraud rejected
        var payment3 = await session.LoadAsync<PaymentVerification>(payment3Id);
        payment3.Should().NotBeNull();
        payment3!.Id.Should().Be(payment3Id);
        payment3.Status.Should().Be(PaymentStatus.Rejected);

        // Assert Payment 4: Pending
        var payment4 = await session.LoadAsync<PaymentVerification>(payment4Id);
        payment4.Should().NotBeNull();
        payment4!.Id.Should().Be(payment4Id);
        payment4.Status.Should().Be(PaymentStatus.Pending);
    }
}
