namespace ECommerce.Domain.Core.Outbox;

public interface IOutbox
{
    Task Enqueue(object message);
}
