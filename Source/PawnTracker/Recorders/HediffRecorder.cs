using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class HediffRecorder : RecorderBase<HediffAddedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<HediffAddedEvent>(e =>
        {
            if (e.Hediff.def == HediffDefOf.Anesthetic)
                CreateRecord(e);
        });
    }

    public override void CreateRecord(HediffAddedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var desc = HistoryRecordDefOf.Anesthetized.Description(e.Pawn)
            .AddRule("Anesthetic", e.Hediff)
            .Format();

        AddRecord(HistoryRecordDefOf.Anesthetized, e.Pawn, desc);
    }

    public void Test(TestScenario scenario)
    {
        var patient = scenario.Pawn()
            .ThatMatches(ShouldRecord)
            .ThatMatches(p => p.health.hediffSet.hediffs.All(h => h.def != HediffDefOf.Anesthetic))
            .AddHediff(HediffDefOf.Anesthetic)
            .CreateSingle();
        scenario.OpenHistoryRecordTab(patient);

        Expect.That(patient).ToHaveHistoryRecord("[PAWN] was put under anesthetic.");
    }

    public void TestInvert(TestScenario scenario)
    {
        var patient = scenario.Pawn()
            .ThatMatches(ShouldRecord)
            .FullHeal()
            .CreateSingle();
        scenario.OpenHistoryRecordTab(patient);

        Expect.That(patient).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.Anesthetized);
    }
}
