using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class WalkNakedRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<WalkNakedEvent>(e =>
        {
            HandleWalkNakedEvent(e);
        });
    }

    private void HandleWalkNakedEvent(WalkNakedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var recordDef = HistoryRecordDefOf.WalkNaked;
        var recentRecords = GeRecordsOfType(e.Pawn, recordDef).Take(3).ToList();

        if (IsTooSoon(recentRecords.FirstOrDefault()))
        {
            Log.Message($"[PawnHistory] Skipped recording {e.Pawn} WalkNaked event | TooSoon");
            return;
        }

        var desc = recordDef.ResolveDescription("walkNaked", e.Pawn)
            .IncludePawnGrammar()
            .Resolve();

        if (recentRecords.Any(r => LangUtility.IsTooSimilar(desc, r.description, 0.7f)))
        {
            Log.Message($"[PawnHistory] Skipped recording {e.Pawn} WalkNaked event | TooSimilar | \"{desc}\"");
            return;
        }

        AddRecord(recordDef, e.Pawn, desc);
    }

    private bool IsTooSoon(HistoryRecord lastRecord)
    {
        if (lastRecord == null)
            return false;

        return GenTicks.TicksAbs - lastRecord.date < GenDate.DaysToTicks(1);
    }

    public override void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(15).Colonist().StripNaked().Create();

        foreach (var pawn in pawns)
        {
            TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
            TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
            TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
            TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
            TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
            TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
        }
    }
}
