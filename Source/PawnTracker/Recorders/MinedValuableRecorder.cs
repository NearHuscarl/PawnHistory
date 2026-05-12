using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class MinedValuableRecorder : HistoryTaleRecorder<MinedValuableEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<MinedValuableEvent>(CreateRecord);
    }

    public override void CreateRecord(MinedValuableEvent e)
    {
        var (pawn, mineableThing) = e;
        var recordDef = HistoryRecordDefOf.MinedValuable;
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .AddRule("Material", mineableThing)
            .Resolve();

        if (!ShouldRecordTale(pawn, recordDef, desc))
            return;

        AddRecord(recordDef, pawn, desc);
    }

    [SkipTest]
    public void Test(TestScenario scenario)
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
