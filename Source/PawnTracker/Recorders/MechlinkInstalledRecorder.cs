using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class MechlinkInstalledRecorder : RecorderBase<MechlinkInstalledEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<MechlinkInstalledEvent>(CreateRecord);
    }

    public override void CreateRecord(MechlinkInstalledEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var recordDef = HistoryRecordDefOf.MechlinkInstalled;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .Resolve();
        AddRecord(recordDef, e.Pawn, desc);
    }

    [RequiresBiotech]
    public void Test(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .ThatMatches(p => !p.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant))
            .AddHediff(HediffDefOf.MechlinkImplant)
            .CreateSingle();

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.MechlinkInstalled, "[PAWN] installed a mechlink and became a mechanitor. [He] could now create and control mechanoids.");
    }
}
