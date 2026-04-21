using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using System.Linq;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class TechprintAppliedRecorder : RecorderBase<TechprintAppliedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<TechprintAppliedEvent>(CreateRecord);
    }

    public override void CreateRecord(TechprintAppliedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var recordDef = HistoryRecordDefOf.TechprintApplied;
        var desc = recordDef.Description(e.Pawn)
            .AddRule("Project", e.Project.label.Colorize(ColoredText.SubtleGrayColor))
            .AddRule("Xp", Mathf.RoundToInt(e.XpGained))
            .Format();

        AddRecord(recordDef, e.Pawn, desc);
    }

    // TODO: test with royalty
    [RequiresRoyalty]
    public void Test(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var project = DefDatabase<ResearchProjectDef>.AllDefsListForReading
            .First(p => p.Techprint != null && !p.IsFinished);

        Find.ResearchManager.ApplyTechprint(project, pawn);

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] applied a techprint to [Project], gaining [n] XP in intellectual.", HistoryRecordDefOf.TechprintApplied);
    }
}
