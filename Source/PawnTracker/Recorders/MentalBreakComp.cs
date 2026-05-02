using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using RimWorld;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Recorders;

public abstract class MentalBreakComp : RecordComp<MentalBreakRecorder>
{
    public record BuildInput(Pawn Pawn, MentalBreakReason Reason, MentalBreakDef MentalBreak, MentalStateDef MentalStateDef, MentalState MentalState, Pawn Target, Quest Quest);

    public abstract bool Match(BuildInput input);

    public virtual HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder;
    }

    public virtual IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        yield break;
    }
}
