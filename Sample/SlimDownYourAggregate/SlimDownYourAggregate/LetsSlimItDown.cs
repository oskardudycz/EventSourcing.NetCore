namespace SlimDownYourAggregate.Slimmed;

using System;
using System.Collections.Generic;
using System.Linq;

public record ChemicalReactionModel(
    Guid Id,
    List<ReactionParticipant> Participants,
    ReactionState State
);

public static class ChemicalReactionModelExtensions
{
    public static ChemicalReaction ToChemicalReaction(this ChemicalReactionModel model) =>
        model.State switch
        {
            ReactionState.NotStarted => new ChemicalReaction.NotStarted(),
            ReactionState.InProgress => new ChemicalReaction.Started(),
            ReactionState.Completed => new ChemicalReaction.Finished(),
            _ => throw new ArgumentOutOfRangeException()
        };


    public static ChemicalReactionModel Evolve(this ChemicalReactionModel model, ChemicalReactionEvent @event)
    {
        return @event switch
        {
            ChemicalReactionEvent.Initiated initiated =>
                new ChemicalReactionModel(initiated.Id, initiated.Participants, ReactionState.NotStarted),

            ChemicalReactionEvent.Started started =>
                model with { State = ReactionState.InProgress },

            ChemicalReactionEvent.Finished finished =>
                model with { State = ReactionState.Completed },

            _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
        };
    }
}

public record ReactionParticipantModel(
    Guid Id,
    string ChemicalName,
    double Moles,
    ParticipantType Type
);

public abstract record ChemicalReactionCommand
{
    public record Initiate(
        Guid Id,
        List<ReactionParticipant> Participants,
        double InitialMoles,
        double FinalMoles
    ): ChemicalReactionCommand;

    public record Start(
        DateTimeOffset StartedAt
    ): ChemicalReactionCommand;

    public record Finish(
        DateTimeOffset FinishedAt
    ): ChemicalReactionCommand;
}

public abstract record ChemicalReactionEvent
{
    public record Initiated(
        Guid Id,
        List<ReactionParticipant> Participants,
        double InitialMoles,
        double FinalMoles
    ): ChemicalReactionEvent;

    public record Started(
        DateTimeOffset StartedAt
    ): ChemicalReactionEvent;

    public record Finished(
        DateTimeOffset FinishedAt
    ): ChemicalReactionEvent;
}



public static class ChemicalReactionService
{
    public static ChemicalReactionEvent Decide(ChemicalReactionCommand command, ChemicalReaction state)
    {
        return command switch
        {
            ChemicalReactionCommand.Initiate initiate =>
            {
                if(state is not ChemicalReaction.NotInitiated)
                    throw new InvalidOperationException();

                return new ChemicalReaction.Initiated(command...);
            },
            ChemicalReactionCommand.Start start =>
            {
                if(state is not ChemicalReaction.Initiated initiated)
                    throw new InvalidOperationException();

                return initiated.Start(command...);
            }
            ChemicalReactionCommand.Finish finish =>
            {
                if(state is not ChemicalReaction.Started started)
                    throw new InvalidOperationException();

                return started.StopReaction(command...);
            }
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        }
    }
}

public abstract record ChemicalReaction
{
    public record NotInitiated;
    public record Initiated: ChemicalReaction
    {
        public static ChemicalReactionEvent.Initiated Initiate(Guid id, List<ReactionParticipant> participants)
        {
            if (participants.Count < 2)
                throw new ArgumentException("A chemical reaction must have at least two participants.");

            var initialMoles = participants.Where(p => p.Type == ParticipantType.Reactant).Sum(p => p.Moles);
            var finalMoles = participants.Where(p => p.Type == ParticipantType.Product).Sum(p => p.Moles);

            if (!IsMoleConservationSatisfied(initialMoles, finalMoles))
                throw new InvalidOperationException("The total number of moles in the reactants must be conserved.");

            return new ChemicalReactionEvent.Initiated(id, participants, initialMoles, finalMoles);
        }

        public ChemicalReactionEvent.Started Start(DateTimeOffset now) =>
            new ChemicalReactionEvent.Started(now);

        private static bool IsMoleConservationSatisfied(double initialMoles, double finalMoles) =>
            Math.Abs(initialMoles - finalMoles) < 1e-6;
    }

    public record Started: ChemicalReaction
    {
        public ChemicalReactionEvent.Finished StopReaction(DateTimeOffset now) =>
            new ChemicalReactionEvent.Finished(now);
    }

    public record Finished: ChemicalReaction;
}

public class ReactionParticipant
{
    public double Moles { get; private set; }
    public ParticipantType Type { get; private set; }

    public ReactionParticipant(double moles, ParticipantType type)
    {
        Moles = moles;
        Type = type;
    }

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
