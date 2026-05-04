using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class MiscarriedRecorder : RecorderBase<MiscarriedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<MiscarriedEvent>(CreateRecord);
    }

    public override void CreateRecord(MiscarriedEvent e)
    {
        if (!ShouldRecord(e.Carrier))
            return;

        var desc = HistoryRecordDefOf.Miscarried.Description(e.Carrier)
            .AddConstant("reason", e.Reason)
            .Resolve();

        AddRecord(HistoryRecordDefOf.Miscarried, e.Carrier, desc);
    }

    [RequiresBiotech]
    public void TestStarvation(TestScenario scenario)
    {
        var hediff = (Hediff)null;
        var carrier = scenario.Pawn()
            .Colonist()
            .SetGender(Gender.Female)
            .AddHediff(HediffDefOf.Malnutrition, hediffCreated: h => h.Severity = 0.2f)
            .AddHediff(HediffDefOf.PregnantHuman, hediffCreated: h => hediff = h)
            .Do(pawn => pawn.needs.food.CurLevel = 0f)
            .CreateSingle();

        var oldValue = Find.Storyteller.difficulty.babiesAreHealthy;
        Find.Storyteller.difficulty.babiesAreHealthy = false;
        for (var i = 0; i < 300; i++)
            hediff.TickInterval(int.MaxValue);
        Find.Storyteller.difficulty.babiesAreHealthy = oldValue;
        
        Expect.That(carrier).ToHaveHistoryRecord(HistoryRecordDefOf.Miscarried, "[PAWN] miscarried due to starvation.", exactMatch: true);
    }

    [RequiresBiotech]
    public void TestPoorHealth(TestScenario scenario)
    {
        var hediff = (Hediff)null;
        var carrier = scenario.Pawn()
            .Enemy() // make ShouldSendNotificationAbout return false
            .SetGender(Gender.Female)
            .AddHediff(HediffDefOf.PregnantHuman, hediffCreated: h => hediff = h)
            .Do(p => HealthUtility.DamageUntilDowned(p))
            .CreateSingle();

        var oldValue = Find.Storyteller.difficulty.babiesAreHealthy;
        Find.Storyteller.difficulty.babiesAreHealthy = false;
        for (var i = 0; i < 300; i++)
            hediff.TickInterval(int.MaxValue);
        Find.Storyteller.difficulty.babiesAreHealthy = oldValue;

        Expect.That(carrier).ToHaveHistoryRecord(HistoryRecordDefOf.Miscarried, "[PAWN] miscarried due to poor health.", exactMatch: true);
    }
}
