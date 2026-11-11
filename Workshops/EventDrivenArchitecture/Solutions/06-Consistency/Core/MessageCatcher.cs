using FluentAssertions;

namespace Consistency.Core;

public class MessageCatcher
{
    public List<object> Published { get; } = [];

    private readonly Lock publishedLock = new();

    public void Catch(object message)
    {
        lock (publishedLock)
        {
            Published.Add(message);
        }
    }

    public void Reset() => Published.Clear();

    public void ShouldNotReceiveAnyMessage() =>
        Published.Should().BeEmpty();

    public void ShouldReceiveSingleMessage<T>(T message)
    {
        Published.Should().HaveCount(1);
        Published.OfType<T>().Should().HaveCount(1);
        Published.Single().Should().BeEquivalentTo(message);
    }

    public void ShouldReceiveMessages(object[] messages) =>
        Published.Should().BeEquivalentTo(messages, options => options.WithStrictOrdering());

    public void ShouldReceiveMessagesInAnyOrder(object[] messages) =>
        Published.Should().BeEquivalentTo(messages);
}
