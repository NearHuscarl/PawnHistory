using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class DrugAddictedRecorder : RecorderBase<DrugAddictedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<DrugAddictedEvent>(CreateRecord);
    }

    public override void CreateRecord(DrugAddictedEvent e)
    {
        var (pawn, hediff, chemical) = e;
        
        if (!ShouldRecord(pawn))
            return;
        
        var recordDef = HistoryRecordDefOf.DrugAddicted;
        var desc = recordDef.Description(pawn)
            .AddRule("Addiction", hediff, addSubsymbols: true)
            .AddRule("Chemical", chemical)
            .Resolve();
        AddRecord(recordDef, pawn, desc);
    }

    public void Test(TestScenario scenario)
    {
        var drug1 = scenario.Thing(ThingDefOf.WakeUp).Create();
        var pawn1 = scenario.Pawn()
            .FullHeal()
            .AddHediff(DefLookup.Hediff.WakeUpTolerance, hediffCreated: h => h.Severity = 1f)
            .ForceAddictionTo(drug1)
            .CreateSingle();
        
        var drug2 = scenario.Thing(ThingDefOf.Beer).Create();
        var pawn2 = scenario.Pawn()
            .FullHeal()
            .AddHediff(DefLookup.Hediff.AlcoholTolerance, hediffCreated: h => h.Severity = 1f)
            .ForceAddictionTo(drug2)
            .CreateSingle();
        
        Expect.That(pawn1).ToHaveHistoryRecord("[PAWN] developed a wake-up addiction.", HistoryRecordDefOf.DrugAddicted);
        Expect.That(pawn2).ToHaveHistoryRecord("[PAWN] developed an alcohol addiction.", HistoryRecordDefOf.DrugAddicted);
    }
}
