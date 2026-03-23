using PawnHistory.Source.Helper;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class HistoryTaleRecorder : RecorderBase
{
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
            return false;
        }

        if (recentRecords.Any(r => LangUtility.IsTooSimilar(description, r.description, 0.7f)))
        {
            Log.Message($"[PawnHistory] Skipped recording {pawn}'s {recordDef.defName} event | TooSimilar | \"{description}\"");
            return false;
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
