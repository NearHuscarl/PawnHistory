using PawnHistory.Source.Helper;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class HistoryTaleRecorder : RecorderBase
{
    protected bool skipDateCheck = false;
    protected bool skipOverlapCheck = false;

    protected float DaysToRecordAgain { get; set; } = 1f;

    public override void Register() { }

    protected bool ShouldRecordTale(Pawn pawn, HistoryRecordDef recordDef, string description)
    {
        if (!ShouldRecord(pawn))
            return false;

        var recentRecords = GeRecordsOfType(pawn, recordDef).Take(3).ToList();

        if (IsTooSoonToRecordAgain(recentRecords.FirstOrDefault()))
        {
            Log.Message($"[PawnHistory] Skipped recording {pawn}'s {recordDef.defName} event | TooSoon");
            if (!skipDateCheck) return false;
        }

        foreach (var record in recentRecords)
        {
            var overlapScore = LangUtility.GetOverlapScore(description, record.description);
            if (overlapScore < 0.7f)
                continue;

            Log.Message($"[PawnHistory] Skipped recording {pawn}'s {recordDef.defName} event | TooSimilar({overlapScore})\n\n{description}\n\n{record.description}");
            if (!skipOverlapCheck) return false;
        }
        
        return true;
    }

    protected bool IsTooSoonToRecordAgain(HistoryRecord lastRecord)
    {
        if (lastRecord == null)
            return false;

        return GenTicks.TicksAbs - lastRecord.date < GenDate.DaysToTicks(DaysToRecordAgain);
    }
}
