using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class HediffRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<HediffPostAddEvent>(e =>
        {
            var pawn = e.Pawn;
            var hediff = e.Hediff;

            if (!ShouldRecord(pawn))
                return;

            if (hediff.def == HediffDefOf.Anesthetic)
                HandleAnesthetizedEvent(pawn, hediff);
        });
    }

    private void HandleAnesthetizedEvent(Pawn pawn, Hediff hediff)
    {
        var desc = HistoryRecordDefOf.Anesthetized.ResolveDescription(pawn)
            .AddRule("ANESTHETIC", hediff)
            .Resolve();

        AddRecord(HistoryRecordDefOf.Anesthetized, pawn, desc);
    }

    public override void Test(TestScenario scenario)
    {
        var patient = scenario.Pawn()
            .ThatMatches(ShouldRecord)
            .ThatMatches(p => p.health.hediffSet.hediffs.All(h => h.def != HediffDefOf.Anesthetic))
            .Do(p => p.health.AddHediff(HediffDefOf.Anesthetic))
            .CreateSingle();
        scenario.OpenHistoryRecordTab(patient);
    }
}
