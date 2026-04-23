using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public abstract class QuestPawnArrivedComp : RecordComp<QuestPawnArrivedRecorder>
{
    public abstract bool Match(Quest quest);
    
    public virtual HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, Quest quest, Pawn pawn, List<Pawn> questPawns)
    {
        return builder;
    }
    
    public virtual IEnumerable<Thing> GetConcerns(List<Pawn> questPawns)
    {
        yield break;
    }
}
