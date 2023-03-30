namespace Core.Decider;

public record Decider<TState, TCommand, TEvent>(
    Func<TCommand, TState, TEvent[]> Decide,
    Func<TState, TEvent, TState> Evolve,
    Func<TState> GetInitialState
) {
}
