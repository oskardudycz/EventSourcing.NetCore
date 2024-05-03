namespace IntroductionToEventSourcing.BusinessLogic.Mixed;

public interface IAggregate
{
    Guid Id { get; }

    void Evolve(object @event) { }
}
