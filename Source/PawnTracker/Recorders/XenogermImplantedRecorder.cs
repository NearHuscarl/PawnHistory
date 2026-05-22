using System;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class XenogermImplantedRecorder : RecorderBase<XenogermImplantedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<XenogermImplantedEvent>(CreateRecord);
    }

    public override void CreateRecord(XenogermImplantedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var recordDef = HistoryRecordDefOf.XenogermImplanted;
        var genes = OrderByImpact(e.Genes);
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddRule("XenotypeName", e.XenotypeName)
            .AddRule("OldXenotypeName", e.OldXenotypeName, addSubsymbols: true)
            .AddConstant("hasOldXenotypeName", !e.OldXenotypeName.NullOrEmpty())
            .AddRule("Genes", LangUtility.FormatList(genes, gene => gene.label.Colorize(ColoredText.GeneColor), "NH_PH_OtherGene".TranslateSimple()))
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc);
    }

    private static List<GeneDef> OrderByImpact(List<GeneDef> genes)
    {
        return genes
            .OrderByDescending(GeneImpact)
            .ThenBy(g => g.label)
            .ToList();
    }

    private static int GeneImpact(GeneDef gene) => gene.biostatArc * 100 + gene.biostatCpx + Math.Abs(gene.biostatMet);

    [RequiresBiotech]
    public void Test(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().SetXenotype(Extra.XenotypeDefOf.Dirtmole).CreateSingle();
        var xenogerm = scenario.Thing(ThingDefOf.Xenogerm).MakeXenogerm("Archivist", Extra.XenotypeIconDefOf.Crown, [
            GeneDefOf.Deathless,
            Extra.GeneDefOf.TotalHealing,
            Extra.GeneDefOf.Deathrest,
            GeneDefOf.Hemogenic
        ]);

        GeneUtility.ImplantXenogermItem(pawn, xenogerm);

        Expect.That(pawn).ToHaveHistoryRecord(
            HistoryRecordDefOf.XenogermImplanted,
            "[PAWN], who was once a dirtmole, was implanted with the Archivist xenogerm, gaining deathless, scarless and 2 other genes.");
    }

    [RequiresBiotech]
    public void Test2(TestScenario scenario)
    {
        var iconDef = Extra.XenotypeIconDefOf.Crown;
        var pawn = scenario.Pawn()
            .Colonist()
            .SetXenotype("Old Custom", iconDef, [GeneDefOf.Hemogenic])
            .CreateSingle();

        var xenogerm = scenario.Thing(ThingDefOf.Xenogerm).MakeXenogerm("Archivist", iconDef, [
            GeneDefOf.Deathless,
            Extra.GeneDefOf.TotalHealing,
            Extra.GeneDefOf.Deathrest
        ]);

        GeneUtility.ImplantXenogermItem(pawn, xenogerm);

        Expect.That(pawn).ToHaveHistoryRecord(
            HistoryRecordDefOf.XenogermImplanted,
            "[PAWN] was implanted with the Archivist xenogerm, gaining deathless, scarless and 1 other gene.");
        var record = pawn.HistoryRecords.Last(r => r.def == HistoryRecordDefOf.XenogermImplanted);
        Expect.That(record.description).Not().Contain("who was once");
    }
}
