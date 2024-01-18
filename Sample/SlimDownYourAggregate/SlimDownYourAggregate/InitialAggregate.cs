namespace SlimDownYourAggregate.Initial;

using System;
using System.Collections.Generic;
using System.Linq;

public class ChemicalReaction
{
    public Guid Id { get; private set; }
    public List<ReactionParticipant> Participants { get; private set; }
    public ReactionState State { get; private set; }

    public ChemicalReaction(Guid id, List<ReactionParticipant> participants)
    {
        if (participants.Count < 2)
            throw new ArgumentException("A chemical reaction must have at least two participants.");

        Id = id;
        Participants = participants;
        State = ReactionState.NotStarted;
    }

    public void StartReaction()
    {
        if (State != ReactionState.NotStarted)
            throw new InvalidOperationException("The reaction can be started only if it is in the 'NotStarted' state.");

        State = ReactionState.InProgress;
    }

    public void StopReaction()
    {
        if (State != ReactionState.InProgress)
            throw new InvalidOperationException("The reaction can be stopped only if it is in the 'InProgress' state.");

        if (!IsMoleConservationSatisfied())
            throw new InvalidOperationException("The total number of moles in the reactants must be conserved.");

        State = ReactionState.Completed;
    }

    private bool IsMoleConservationSatisfied()
    {
        double initialMoles = Participants.Where(p => p.Type == ParticipantType.Reactant).Sum(p => p.Moles);
        double finalMoles = Participants.Where(p => p.Type == ParticipantType.Product).Sum(p => p.Moles);

        return Math.Abs(initialMoles - finalMoles) < 1e-6;
    }
}

public enum ReactionState
{
    NotStarted,
    InProgress,
    Completed
}

public class ReactionParticipant
{
    public Guid Id { get; private set; }
    public string ChemicalName { get; private set; }
    public double Moles { get; private set; }
    public ParticipantType Type { get; private set; }

    public ReactionParticipant(Guid id, string chemicalName, double moles, ParticipantType type)
    {
        Id = id;
        ChemicalName = chemicalName;
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
