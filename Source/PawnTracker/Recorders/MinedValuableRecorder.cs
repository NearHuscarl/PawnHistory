using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class MinedValuableRecorder : HistoryTaleRecorder
{
    public override void Register()
    {
        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale.def != TaleDefOf.MinedValuable)
                return;

            HandleMinedValuableEvent(e);
        });
    }

    private void HandleMinedValuableEvent(TaleRecordedEvent e)
    {
        var recordDef = HistoryRecordDefOf.MinedValuable;
        var mineableThing = e.Params[0] as ThingDef;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddRule("Material", mineableThing)
            .Resolve();

        if (!ShouldRecordTale(e.Pawn, recordDef, desc))
            return;

        AddRecord(recordDef, e.Pawn, desc);
    }

    public override void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(15).Colonist().Execute();
        var mineables = new List<ThingDef>() { ThingDefOf.MineableGold, ThingDefOf.MineableSteel, ThingDefOf.MineableComponentsIndustrial };

        foreach (var pawn in pawns)
        {
            TaleRecorder.RecordTale(TaleDefOf.MinedValuable, pawn, mineables.RandomElement());
            TaleRecorder.RecordTale(TaleDefOf.MinedValuable, pawn, mineables.RandomElement());
            TaleRecorder.RecordTale(TaleDefOf.MinedValuable, pawn, mineables.RandomElement());
        }
    }
}
