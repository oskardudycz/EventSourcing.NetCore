using FluentAssertions;

namespace BusinessProcesses.Core;

public class EventCatcher
{
    public List<object> Published { get; } = [];

    public void Catch(object message) =>
        Published.Add(message);

    public void Reset() => Published.Clear();

    public void ShouldNotReceiveAnyMessage() =>
        Published.Should().BeEmpty();

    public void ShouldReceiveSingleMessage<T>(T message)
    {
        Published.Should().HaveCount(1);
        Published.OfType<T>().Should().HaveCount(1);
        Published.Single().Should().Be(message);
    }

    public void ShouldReceiveMessages(object[] messages) =>
        Published.Should().BeEquivalentTo(messages);
}
