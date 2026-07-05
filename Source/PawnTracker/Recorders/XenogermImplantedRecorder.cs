using System;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

// Reason this is its own record rather than a comp of SurgeryRecorder:
// - ImplantXenogerm's surgeryOutcomeEffect is inconsequential and only affect recovering time -> not worth reporting
// - Does not interact with hediffs hence do not need to integrate with SurgeryContext
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
        var geneList = LangUtility.FormatLabeledList(
            genes,
            gene => gene.label.Colorize(ColoredText.GeneColor),
            "NH_PH_Gene".Translate(),
            "NH_PH_Genes".Translate(),
            "NH_PH_OtherGene".Translate()
        );
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddRule("XenotypeName", e.XenotypeName)
            .AddRule("OldXenotypeName", e.OldXenotypeName, addSubsymbols: true)
            .AddRule("Donor", e.Donor)
            .AddConstant("hasOldXenotypeName", !e.OldXenotypeName.NullOrEmpty())
            .AddConstant("hasDonor", e.Donor != null)
            .AddRule("Genes", geneList)
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc, [e.Donor]);
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
            "[PAWN] was implanted with the Archivist xenogerm, gaining deathless, scarless and deathrest genes.");
        var record = pawn.HistoryRecords.Last(r => r.def == HistoryRecordDefOf.XenogermImplanted);
        Expect.That(record.description).Not().Contain("who was once");
    }

    [RequiresBiotech]
    public void TestReimplanted(TestScenario scenario)
    {
        var donor = scenario.Pawn()
            .Colonist()
            .SetXenotype("MasterRace", Extra.XenotypeIconDefOf.Crown, [GeneDefOf.Deathless])
            .CreateSingle();
        var recipient = scenario.Pawn()
            .Colonist()
            .SetXenotype(Extra.XenotypeDefOf.Dirtmole)
            .CreateSingle();

        GeneUtility.ReimplantXenogerm(donor, recipient);

        Expect.That(recipient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.XenogermImplanted,
            Description = "[PAWN], who was once a dirtmole, was implanted with the MasterRace xenogerm by [Donor], gaining deathless gene.",
            Concerns = [donor]
        });
    }
}
