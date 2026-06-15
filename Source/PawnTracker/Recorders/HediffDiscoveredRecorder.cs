using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

// HediffCompProperties_Discoverable, currently only used in DrugOverdose
public class HediffDiscoveredRecorder : RecorderBase<HediffDiscoveredEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<HediffDiscoveredEvent>(CreateRecord);
    }
    
    // TODO: will be handled by a separate recorder
    private static bool ShouldIgnore(Hediff hediff)
    {
        return hediff.def == HediffDefOf.WoundInfection || hediff.def == HediffDefOf.ScariaInfection;
    }

    public override void CreateRecord(HediffDiscoveredEvent e)
    {
        var (pawn, hediff, _) = e;

        if (ShouldIgnore(hediff))
            return;
        
        if (!ShouldRecord(pawn))
            return;

        var recordDef = HistoryRecordDefOf.HediffDiscovered;
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .AddRule("Hediff", hediff.LabelNounPretty())
            .Resolve();

        AddRecord(recordDef, pawn, desc);
    }

    public void TestOverdose(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .FullHeal()
            .AddHediff(HediffDefOf.DrugOverdose, hediffCreated: h => h.Severity = 0.5f)
            .CreateSingle();

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.HediffDiscovered, "[PAWN] suffered from a drug overdose.");
    }
}
