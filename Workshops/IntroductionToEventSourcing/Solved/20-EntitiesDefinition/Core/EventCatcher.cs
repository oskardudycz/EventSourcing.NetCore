using Shouldly;

namespace EntitiesDefinition.Core;

public class EventCatcher
{
    public List<object> Published { get; } = [];

    public void Catch(object message) =>
        Published.Add(message);

    public void Reset() => Published.Clear();

    public void ShouldNotReceiveAnyEvent() =>
        Published.ShouldBeEmpty();


    public void ShouldReceiveEvent<T>(T message) where T: notnull
    {
        Published.ShouldContain(message);
    }

    public void ShouldReceiveSingleEvent<T>(T message)
    {
        Published.Count().ShouldBe(1);
        Published.OfType<T>().Count().ShouldBe(1);
        Published.Single().ShouldBe(message);
    }
}
