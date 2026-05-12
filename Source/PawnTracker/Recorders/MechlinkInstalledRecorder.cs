using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.HistoryBackfill;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class MechlinkInstalledRecorder : RecorderBase<MechlinkInstalledEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<MechlinkInstalledEvent>(CreateRecord);
    }

    internal override IEnumerable<HistoryBackfillDefinition> GetBackfillDefinitions()
    {
        if (!ModsConfig.BiotechActive)
            yield break;

        yield return new HistoryBackfillDefinition(HistoryRecordDefOf.MechlinkInstalled)
            .AddHard(
                new MinimumAgeRule(13f),
                new MaximumCountRule(1),
                new LogicalGateRule((_, _) => ModsConfig.BiotechActive))
            .AddSoft(new AgeCurveSoftRule([
                new CurvePoint(13f, 0.01f),
                new CurvePoint(16f, 0.08f),
                new CurvePoint(20f, 0.3f),
                new CurvePoint(28f, 1f),
                new CurvePoint(45f, 1.1f),
                new CurvePoint(70f, 0.4f)
            ]));
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
