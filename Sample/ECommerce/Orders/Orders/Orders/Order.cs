using Core.Aggregates;
using Orders.Orders.CancellingOrder;
using Orders.Products;

namespace Orders.Orders;

using static OrderEvent;

public class Order: Aggregate<OrderEvent>
{
    public readonly TimeSpan DefaultTimeOut = TimeSpan.FromMinutes(5);

    public IReadOnlyList<PricedProductItem> ProductItems { get; private set; } = default!;

    public decimal TotalPrice { get; private set; }

    public OrderStatus Status { get; private set; }

    public Guid? PaymentId { get; private set; }

    public static Order Initialize(
        Guid orderId,
        Guid clientId,
        IReadOnlyList<PricedProductItem> productItems,
        decimal totalPrice,
        DateTimeOffset now
    ) =>
        new(
            orderId,
            clientId,
            productItems,
            totalPrice,
            now
        );

    public Order() { }

    private Order(
        Guid id,
        Guid clientId,
        IReadOnlyList<PricedProductItem> productItems,
        decimal totalPrice,
        DateTimeOffset now,
        TimeSpan? timeout = null
    ) =>
        Enqueue(
            OrderInitiated.From(
                id,
                clientId,
                productItems,
                totalPrice,
                now,
                now.Add(timeout ?? DefaultTimeOut)
            )
        );

    public void RecordPayment(Guid paymentId, DateTimeOffset recordedAt) =>
        Enqueue(
            OrderPaymentRecorded.Create(
                Id,
                paymentId,
                ProductItems,
                TotalPrice,
                recordedAt
            )
        );

    public void Complete(DateTimeOffset now)
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException($"Cannot complete a not paid order.");

        Enqueue(
            OrderCompleted.Create(Id, now)
        );
    }

    public void Cancel(OrderCancellationReason cancellationReason, DateTimeOffset now)
    {
        // QUESTION TO THE READER: Is throwing Exception here fine or not?
        if (OrderStatus.Closed.HasFlag(Status))
            throw new InvalidOperationException("Cannot cancel a closed order.");

        Enqueue(
            OrderCancelled.Create(Id, PaymentId, cancellationReason, now)
        );
    }

    public override void Apply(OrderEvent @event)
    {
        switch (@event)
        {
            case OrderInitiated orderInitiated:
                Id = orderInitiated.OrderId;
                ProductItems = orderInitiated.ProductItems;
                TotalPrice = orderInitiated.TotalPrice;
                Status = OrderStatus.Opened;
                break;
            case OrderPaymentRecorded paymentRecorded:
                PaymentId = paymentRecorded.PaymentId;
                Status = OrderStatus.Paid;
                break;
            case OrderCompleted:
                Status = OrderStatus.Completed;
                break;
            case OrderCancelled:
                Status = OrderStatus.Cancelled;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(@event));
        }
    }
}
