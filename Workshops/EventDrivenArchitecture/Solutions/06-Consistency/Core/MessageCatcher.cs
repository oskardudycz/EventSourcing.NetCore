using System.Text.Json;
using Shouldly;

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
        Published.ShouldBeEmpty();

    public void ShouldReceiveSingleMessage<T>(T message)
    {
        Published.Count().ShouldBe(1);
        Published.OfType<T>().Count().ShouldBe(1);
        Published.Single().ShouldBeEquivalentTo(message);
    }

    public void ShouldReceiveMessages(object[] messages) =>
        Published.ToArray().ShouldBeEquivalentTo(messages);

    public void ShouldReceiveMessagesInAnyOrder(object[] messages) =>
        Published.Select(Describe).ShouldBe(messages.Select(Describe), ignoreOrder: true);

    private static string Describe(object message) =>
        $"{message.GetType().Name} {JsonSerializer.Serialize(message)}";
}
