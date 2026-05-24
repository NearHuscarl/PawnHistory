using PawnHistory.Source.Helper;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public abstract class HistoryTaleRecorder<TInput> : RecorderBase<TInput> where TInput : class
{
    protected bool SkipDateCheck = false;
    protected bool SkipOverlapCheck = false;
    protected override float DaysToRecordAgain => 1f;

    public abstract override void Register();

    public abstract override void CreateRecord(TInput input);

    protected virtual bool ShouldRecordTale(Pawn pawn, HistoryRecordDef recordDef, string description)
    {
        if (!ShouldRecord(pawn))
            return false;

        if (IsTooSoonToRecordAgain(pawn, recordDef))
        {
            Log.Message($"[PawnHistory] Skipped recording {pawn}'s {recordDef.defName} event | TooSoon");
            if (!SkipDateCheck) return false;
        }

        var recentRecords = GeRecordsOfType(pawn, recordDef).Take(3).ToList();
        foreach (var record in recentRecords)
        {
            var overlapScore = LangUtility.GetOverlapScore(description, record.description);
            if (overlapScore < 0.7f)
                continue;

            Log.Message($"[PawnHistory] Skipped recording {pawn}'s {recordDef.defName} event | TooSimilar({overlapScore})\n\n{description}\n\n{record.description}");
            if (!SkipOverlapCheck) return false;
        }
        
        return true;
    }
}
