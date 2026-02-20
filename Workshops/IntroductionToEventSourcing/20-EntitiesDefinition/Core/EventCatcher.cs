using FluentAssertions;

namespace EntitiesDefinition.Core;

public class EventCatcher
{
    public List<object> Published { get; } = [];

    public void Catch(object message) =>
        Published.Add(message);

    public void Reset() => Published.Clear();

    public void ShouldNotReceiveAnyEvent() =>
        Published.Should().BeEmpty();


    public void ShouldReceiveEvent<T>(T message) where T: notnull
    {
        Published.Should().Contain(message);
    }

    public void ShouldReceiveSingleEvent<T>(T message)
    {
        Published.Should().HaveCount(1);
        Published.OfType<T>().Should().HaveCount(1);
        Published.Single().Should().Be(message);
    }
}
