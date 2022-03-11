namespace Orders.Orders;

public enum OrderStatus
{
    Opened = 1,
    Paid = 2,
    Completed = 4,
    Cancelled = 8,
    Closed = Completed | Cancelled
}
