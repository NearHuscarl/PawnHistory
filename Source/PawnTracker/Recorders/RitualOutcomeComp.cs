using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public abstract class RitualOutcomeComp : RecordComp<RitualOutcomeRecorder>
{
    public record BuildInput(RitualOutcomeCompletedEvent Event);

    public abstract bool Match(BuildInput input);

    public virtual HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder;
    }

    public virtual IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        yield break;
    }

    public virtual bool RecordParticipants { get; protected set; } = false;
}
