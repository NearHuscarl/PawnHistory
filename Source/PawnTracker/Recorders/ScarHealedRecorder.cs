using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class ScarHealedRecorder : RecorderBase<ScarHealedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<ScarHealedEvent>(CreateRecord);
    }

    public override void CreateRecord(ScarHealedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var (pawn, hediff, part, reason) = e;
        var recordDef = HistoryRecordDefOf.ScarHealed;
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .AddRule("Part", part)
            .AddRule("Hediff", hediff, addSubsymbols: true)
            .AddRule("CauseHediff", reason.Hediff)
            .AddRule("CauseGene", reason.Gene)
            .AddConstant("cause", reason.Cause)
            .Resolve();

        AddRecord(recordDef, pawn, desc);
    }

    public void TestLuciferium(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .FullHeal()
            .AddHediff(HediffDefOf.Bite, Extra.BodyPartDefOf.Ear, MakePermanentScar)
            .AddHediff(Extra.HediffDefOf.LuciferiumHigh)
            .CreateSingle();

        pawn.health.hediffSet.GetFirstHediffOfDef(Extra.HediffDefOf.LuciferiumHigh).PostTickInterval(int.MaxValue);

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.ScarHealed, "[PAWN] recovered from a bite scar in [His] left ear thanks to luciferium.");
    }

    [RequiresBiotech]
    public void TestGene(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .FullHeal()
            .AddHediff(HediffDefOf.Bite, Extra.BodyPartDefOf.Ear, MakePermanentScar)
            .Do(p => p.genes.AddGene(Extra.GeneDefOf.TotalHealing, xenogene: false))
            .CreateSingle();

        pawn.genes.GetGene(Extra.GeneDefOf.TotalHealing).TickInterval(int.MaxValue);

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.ScarHealed, "[PAWN] recovered from a bite scar in [His] left ear thanks to scarless gene.");
    }

    private static void MakePermanentScar(Hediff hediff)
    {
        var scar = hediff as Hediff_Injury;
        scar!.Severity = 2f;
        scar.TryGetComp<HediffComp_GetsPermanent>()!.IsPermanent = true;
    }
}
