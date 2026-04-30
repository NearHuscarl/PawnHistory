using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public abstract class QuestPawnArrivedComp : RecordComp<QuestPawnArrivedRecorder>
{
    public record BuildInput(List<Pawn> Pawns, Quest Quest, QuestPawnArrivedMode ArrivalMode, Pawn Pawn, List<Pawn> QuestPawns);
    
    public abstract bool Match(Quest quest);
    
    public virtual HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder;
    }
    
    public virtual IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        yield break;
    }
}
