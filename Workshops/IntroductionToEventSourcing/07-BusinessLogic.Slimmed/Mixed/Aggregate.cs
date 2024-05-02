namespace IntroductionToEventSourcing.BusinessLogic.Slimmed.Mixed;

public interface IAggregate
{
    Guid Id { get; }

    void Evolve(object @event) { }
}
