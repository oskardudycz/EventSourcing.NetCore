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
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public VerificationStatus MerchantLimitStatus { get; set; }
    public VerificationStatus FraudStatus { get; set; }
    public decimal FraudScore { get; set; }
    public PaymentStatus Status { get; set; }
}

public static class DatabaseExtensions
{
    public static void GetAndStore<T>(this Database database, Guid id, Func<T, T> update) where T : class, new()
    {
        var item = database.Get<T>(id) ?? new T();

        database.Store(id, update(item));
    }
}

public class PaymentVerificationProjection(Database database)
{
    public void Handle(EventEnvelope<PaymentRecorded> @event) =>
        database.GetAndStore<PaymentVerification>(@event.Data.PaymentId, item =>
        {
            item.Id = @event.Data.PaymentId;
            item.OrderId = @event.Data.OrderId;
            item.Amount = @event.Data.Amount;

            return item;
        });

    public void Handle(EventEnvelope<MerchantLimitsChecked> @event) =>
        database.GetAndStore<PaymentVerification>(@event.Data.PaymentId, item =>
        {
            item.MerchantLimitStatus = @event.Data.IsWithinLimits
                ? VerificationStatus.Passed
                : VerificationStatus.Failed;

            return item;
        });

    public void Handle(EventEnvelope<FraudScoreCalculated> @event) =>
        database.GetAndStore<PaymentVerification>(@event.Data.PaymentId, item =>
        {
            item.FraudScore = @event.Data.Score;
            item.FraudStatus = @event.Data.IsAcceptable
                ? VerificationStatus.Passed
                : VerificationStatus.Failed;

            return item;
        });

    public void Handle(EventEnvelope<PaymentVerificationCompleted> @event) =>
        database.GetAndStore<PaymentVerification>(@event.Data.PaymentId, item =>
        {
            item.Status = @event.Data.IsApproved
                ? PaymentStatus.Approved
                : PaymentStatus.Rejected;

            return item;
        });
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

        var order1Id = Guid.CreateVersion7();
        var order2Id = Guid.CreateVersion7();
        var order3Id = Guid.CreateVersion7();
        var order4Id = Guid.CreateVersion7();

        var merchant1Id = Guid.CreateVersion7();
        var merchant2Id = Guid.CreateVersion7();

        var fraudCheck1Id = Guid.CreateVersion7();
        var fraudCheck2Id = Guid.CreateVersion7();
        var fraudCheck3Id = Guid.CreateVersion7();

        var eventStore = new EventStore();
        var database = new Database();

        var projection = new PaymentVerificationProjection(database);

        eventStore.Register<PaymentRecorded>(projection.Handle);
        eventStore.Register<MerchantLimitsChecked>(projection.Handle);
        eventStore.Register<FraudScoreCalculated>(projection.Handle);
        eventStore.Register<PaymentVerificationCompleted>(projection.Handle);

        // Payment 1: Approved — all checks pass
        eventStore.Append(payment1Id, new PaymentRecorded(payment1Id, order1Id, 100m));
        eventStore.Append(merchant1Id, new MerchantLimitsChecked(payment1Id, merchant1Id, true));
        eventStore.Append(fraudCheck1Id, new FraudScoreCalculated(payment1Id, 0.1m, true));
        eventStore.Append(payment1Id, new PaymentVerificationCompleted(payment1Id, true));

        // Payment 2: Merchant rejected — exceeds merchant limits
        eventStore.Append(payment2Id, new PaymentRecorded(payment2Id, order2Id, 5000m));
        eventStore.Append(merchant2Id, new MerchantLimitsChecked(payment2Id, merchant2Id, false));
        eventStore.Append(fraudCheck2Id, new FraudScoreCalculated(payment2Id, 0.2m, true));
        eventStore.Append(payment2Id, new PaymentVerificationCompleted(payment2Id, false));

        // Payment 3: Fraud rejected — high fraud score
        eventStore.Append(payment3Id, new PaymentRecorded(payment3Id, order3Id, 200m));
        eventStore.Append(merchant1Id, new MerchantLimitsChecked(payment3Id, merchant1Id, true));
        eventStore.Append(fraudCheck3Id, new FraudScoreCalculated(payment3Id, 0.95m, false));
        eventStore.Append(payment3Id, new PaymentVerificationCompleted(payment3Id, false));

        // Payment 4: Pending — still awaiting fraud check and final decision
        eventStore.Append(payment4Id, new PaymentRecorded(payment4Id, order4Id, 50m));
        eventStore.Append(merchant1Id, new MerchantLimitsChecked(payment4Id, merchant1Id, true));

        // Assert Payment 1: Approved
        var payment1 = database.Get<PaymentVerification>(payment1Id)!;
        payment1.Should().NotBeNull();
        payment1.Id.Should().Be(payment1Id);
        payment1.OrderId.Should().Be(order1Id);
        payment1.Amount.Should().Be(100m);
        payment1.MerchantLimitStatus.Should().Be(VerificationStatus.Passed);
        payment1.FraudStatus.Should().Be(VerificationStatus.Passed);
        payment1.FraudScore.Should().Be(0.1m);
        payment1.Status.Should().Be(PaymentStatus.Approved);

        // Assert Payment 2: Merchant rejected
        var payment2 = database.Get<PaymentVerification>(payment2Id)!;
        payment2.Should().NotBeNull();
        payment2.Id.Should().Be(payment2Id);
        payment2.OrderId.Should().Be(order2Id);
        payment2.Amount.Should().Be(5000m);
        payment2.MerchantLimitStatus.Should().Be(VerificationStatus.Failed);
        payment2.FraudStatus.Should().Be(VerificationStatus.Passed);
        payment2.FraudScore.Should().Be(0.2m);
        payment2.Status.Should().Be(PaymentStatus.Rejected);

        // Assert Payment 3: Fraud rejected
        var payment3 = database.Get<PaymentVerification>(payment3Id)!;
        payment3.Should().NotBeNull();
        payment3.Id.Should().Be(payment3Id);
        payment3.OrderId.Should().Be(order3Id);
        payment3.Amount.Should().Be(200m);
        payment3.MerchantLimitStatus.Should().Be(VerificationStatus.Passed);
        payment3.FraudStatus.Should().Be(VerificationStatus.Failed);
        payment3.FraudScore.Should().Be(0.95m);
        payment3.Status.Should().Be(PaymentStatus.Rejected);

        // Assert Payment 4: Pending
        var payment4 = database.Get<PaymentVerification>(payment4Id)!;
        payment4.Should().NotBeNull();
        payment4.Id.Should().Be(payment4Id);
        payment4.OrderId.Should().Be(order4Id);
        payment4.Amount.Should().Be(50m);
        payment4.MerchantLimitStatus.Should().Be(VerificationStatus.Passed);
        payment4.FraudStatus.Should().Be(VerificationStatus.Pending);
        payment4.FraudScore.Should().Be(0m);
        payment4.Status.Should().Be(PaymentStatus.Pending);
    }
}
