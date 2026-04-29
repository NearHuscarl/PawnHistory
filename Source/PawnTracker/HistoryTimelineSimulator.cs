using PawnHistory.Source.Helper;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public static class HistoryTimelineSimulator
{
    public static void ProcessPawnGenerated(Pawn pawn, HistoryRecord pawnGeneratedRecord)
    {
        var historyComp = CompHistoryManager.GetComp(pawn);
        
        historyComp.PawnGeneratedRecord ??= pawnGeneratedRecord;
        historyComp.PawnGeneratedRecord.pinned = true;

        HistoryBackfillEngine.BackdateGeneratorRecords(pawn, historyComp.PawnGeneratedRecord);
    }
}
