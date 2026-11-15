using System.Text.Json;
using Shouldly;

namespace Consistency.Core;

public class MessageCatcher
{
    public List<object> Published { get; } = [];

    public void Catch(object message) =>
        Published.Add(message);

    public void Reset() => Published.Clear();

    public void ShouldNotReceiveAnyMessage() =>
        Published.ShouldBeEmpty();

    public void ShouldReceiveSingleMessage<T>(T message)
    {
        Published.Count().ShouldBe(1);
        Published.OfType<T>().Count().ShouldBe(1);
        Published.Single().ShouldBe(message);
    }

    public void ShouldReceiveMessages(object[] messages) =>
        Published.Select(Describe).ShouldBe(messages.Select(Describe), ignoreOrder: true);

    private static string Describe(object message) =>
        $"{message.GetType().Name} {JsonSerializer.Serialize(message)}";
}
