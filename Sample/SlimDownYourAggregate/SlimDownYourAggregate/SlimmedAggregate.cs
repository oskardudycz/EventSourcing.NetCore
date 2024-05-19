namespace SlimDownYourAggregate.Slimmed;

using System;
using System.Collections.Generic;

public enum ReactionState
{
    NotStarted,
    InProgress,
    Completed
}

public record ChemicalReactionModel(
    string Id,
    List<ReactionParticipant> Participants,
    ReactionState State
)
{
    public ChemicalReactionModel Evolve(ChemicalReactionModel model, ChemicalReactionEvent @event) =>
        @event switch
        {
            ChemicalReactionEvent.Initiated initiated =>
                new ChemicalReactionModel(initiated.Id, initiated.Participants, ReactionState.NotStarted),
            ChemicalReactionEvent.Started started =>
                model with { State = ReactionState.InProgress },
            ChemicalReactionEvent.Completed finished =>
                model with { State = ReactionState.Completed },
            _ => throw new ArgumentOutOfRangeException(nameof(@event))
        };
}

public static class ChemicalReactionModelExtensions
{
    public static ChemicalReaction ToChemicalReaction(this ChemicalReactionModel model) =>
        model.State switch
        {
            ReactionState.NotStarted => new ChemicalReaction.NotStarted(),
            ReactionState.InProgress => new ChemicalReaction.InProgress(),
            ReactionState.Completed => new ChemicalReaction.Completed(),
            _ => throw new ArgumentOutOfRangeException()
        };


    public static ChemicalReactionModel Evolve(this ChemicalReactionModel model, ChemicalReactionEvent @event) =>
        @event switch
        {
            ChemicalReactionEvent.Initiated initiated =>
                new ChemicalReactionModel(initiated.Id, initiated.Participants, ReactionState.NotStarted),

            ChemicalReactionEvent.Started started =>
                model with { State = ReactionState.InProgress },

            ChemicalReactionEvent.Completed finished =>
                model with { State = ReactionState.Completed },

            _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
        };
}

public record ReactionParticipantModel(
    Guid Id,
    string ChemicalName,
    double Moles,
    ParticipantType Type
);

public abstract record ChemicalReactionCommand(string Id)
{
    public record Initiate(
        string Id,
        List<ReactionParticipant> Participants
    ): ChemicalReactionCommand(Id);

    public record Start(
        string Id,
        DateTimeOffset Now
    ): ChemicalReactionCommand(Id);

    public record Finish(
        string Id,
        DateTimeOffset Now
    ): ChemicalReactionCommand(Id);
}

public abstract record ChemicalReactionEvent
{
    public record Initiated(
        string Id,
        List<ReactionParticipant> Participants,
        double InitialMoles,
        double FinalMoles
    ): ChemicalReactionEvent;

    public record Started(
        DateTimeOffset StartedAt
    ): ChemicalReactionEvent;

    public record Completed(
        DateTimeOffset FinishedAt
    ): ChemicalReactionEvent;
}

public static class ChemicalReactionService
{
    public static ChemicalReactionEvent Decide(ChemicalReactionCommand command, ChemicalReaction state) =>
        command switch
        {
            ChemicalReactionCommand.Initiate initiate =>
                state is ChemicalReaction.NotInitiated
                    ? ChemicalReaction.NotInitiated.Initiate(initiate.Id, initiate.Participants)
                    : throw new InvalidOperationException(),
            ChemicalReactionCommand.Start start =>
                state is ChemicalReaction.NotStarted notStarted
                    ? notStarted.Start(start.Now)
                    : throw new InvalidOperationException(),
            ChemicalReactionCommand.Finish finish =>
                state is ChemicalReaction.InProgress started
                    ? started.Stop(finish.Now)
                    : throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        };
}

public abstract record ChemicalReaction
{
    public record NotInitiated: ChemicalReaction
    {
        public static ChemicalReactionEvent.Initiated Initiate(string id, List<ReactionParticipant> participants)
        {
            if (participants.Count < 2)
                throw new ArgumentException("A chemical reaction must have at least two participants.");

            var initialMoles = participants.Where(p => p.Type == ParticipantType.Reactant).Sum(p => p.Moles);
            var finalMoles = participants.Where(p => p.Type == ParticipantType.Product).Sum(p => p.Moles);

            if (!IsMoleConservationSatisfied(initialMoles, finalMoles))
                throw new InvalidOperationException("The total number of moles in the reactants must be conserved.");

            return new ChemicalReactionEvent.Initiated(id, participants, initialMoles, finalMoles);
        }

        private static bool IsMoleConservationSatisfied(double initialMoles, double finalMoles) =>
            Math.Abs(initialMoles - finalMoles) < 1e-6;
    }

    public record NotStarted: ChemicalReaction
    {
        public ChemicalReactionEvent.Started Start(DateTimeOffset now) => new(now);
    }

    public record InProgress: ChemicalReaction
    {
        public ChemicalReactionEvent.Completed Stop(DateTimeOffset now) => new(now);
    }

    public record Completed: ChemicalReaction;
}

public class ReactionParticipant(double moles, ParticipantType type)
{
    public double Moles { get; private set; } = moles;
    public ParticipantType Type { get; } = type;

    public void UpdateMoles(double newMoles)
    {
        Moles = newMoles;
    }
}

public enum ParticipantType
{
    Reactant,
    Product
}
