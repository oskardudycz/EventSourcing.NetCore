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
    public Guid? OrderId { get; set; }
    public decimal? Amount { get; set; }
    public VerificationStatus MerchantLimitStatus { get; set; }
    public VerificationStatus FraudStatus { get; set; }
    public decimal FraudScore { get; set; }
    public PaymentStatus Status { get; set; }
    public DataQuality DataQuality { get; set; }
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
    public void Handle(EventEnvelope<PaymentRecorded> @event)
    {
        database.GetAndStore<PaymentVerification>(@event.Data.PaymentId, item =>
        {
            item.Id = @event.Data.PaymentId;
            item.OrderId = @event.Data.OrderId;
            item.Amount = @event.Data.Amount;

            return Recalculate(item);
        });
    }

    public void Handle(EventEnvelope<MerchantLimitsChecked> @event)
    {
        database.GetAndStore<PaymentVerification>(@event.Data.PaymentId, item =>
        {
            item.Id = @event.Data.PaymentId;
            item.MerchantLimitStatus = @event.Data.IsWithinLimits
                ? VerificationStatus.Passed
                : VerificationStatus.Failed;

            return Recalculate(item);
        });
    }

    public void Handle(EventEnvelope<FraudScoreCalculated> @event)
    {
        database.GetAndStore<PaymentVerification>(@event.Data.PaymentId, item =>
        {
            item.Id = @event.Data.PaymentId;
            item.FraudScore = @event.Data.Score;
            item.FraudStatus = @event.Data.IsAcceptable
                ? VerificationStatus.Passed
                : VerificationStatus.Failed;

            return Recalculate(item);
        });
    }

    private static PaymentVerification Recalculate(PaymentVerification item)
    {
        // Check if we have all required data
        var hasMerchantCheck = item.MerchantLimitStatus != VerificationStatus.Pending;
        var hasFraudCheck = item.FraudStatus != VerificationStatus.Pending;
        var hasPaymentData = item.OrderId.HasValue && item.Amount.HasValue;

        // Update data quality
        if (hasPaymentData && hasMerchantCheck && hasFraudCheck)
            item.DataQuality = DataQuality.Complete;
        else if (hasMerchantCheck || hasFraudCheck)
            item.DataQuality = DataQuality.Sufficient;
        else
            item.DataQuality = DataQuality.Partial;

        // Derive status based on available data
        // If either check failed, reject immediately (even if we're missing payment data)
        if (item.MerchantLimitStatus == VerificationStatus.Failed ||
            item.FraudStatus == VerificationStatus.Failed)
        {
            item.Status = PaymentStatus.Rejected;
        }
        // If we have both checks and they passed, approve
        else if (hasMerchantCheck && hasFraudCheck)
        {
            item.Status = PaymentStatus.Approved;
        }
        // Otherwise, still pending
        else
        {
            item.Status = PaymentStatus.Pending;
        }

        return item;
    }
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

        var projection = new PaymentVerificationProjection(database);

        eventStore.Register<PaymentRecorded>(projection.Handle);
        eventStore.Register<MerchantLimitsChecked>(projection.Handle);
        eventStore.Register<FraudScoreCalculated>(projection.Handle);

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
