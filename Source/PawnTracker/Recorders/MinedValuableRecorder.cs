using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class MinedValuableRecorder : HistoryTaleRecorder<MinedValuableRecorder.Input>
{
    public record Input(Pawn pawn, ThingDef mineableThing);

    public override void Register()
    {
        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale.def != TaleDefOf.MinedValuable)
                return;

            CreateRecord(new Input(e.Pawn, e.Params[0] as ThingDef));
        });
    }

    public override void CreateRecord(Input input)
    {
        var (pawn, mineableThing) = input;
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
