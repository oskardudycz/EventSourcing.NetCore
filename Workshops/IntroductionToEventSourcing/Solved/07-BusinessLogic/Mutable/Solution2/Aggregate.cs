namespace IntroductionToEventSourcing.BusinessLogic.Mutable.Solution2;

public interface IAggregate
{
    Guid Id { get; }

    void Evolve(object @event) { }
}
