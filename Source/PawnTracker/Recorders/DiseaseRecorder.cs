using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class DiseaseRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<DiseaseEvent>(e =>
        {
            HandleDiseaseEvent(e);
        });
    }

    private void HandleDiseaseEvent(DiseaseEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var recordDef = HistoryRecordDefOf.Disease;
        var diseaseHediffDef = e.IncidentDef.diseaseIncident;
        var part = e.Pawn.health.hediffSet.hediffs.First(h => h.def == diseaseHediffDef && h.ageTicks == 0).Part;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .WithOthers(e.Group.ToList())
            .AddRule("Disease", diseaseHediffDef)
            .AddRule("Part", part)
            .AddConstant("disease", diseaseHediffDef.defName)
            .Resolve();
        AddRecord(recordDef, e.Pawn, desc);
    }

    public override void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(8).Colonist().Execute();
        scenario.Incident("Disease_OrganDecay").Execute();
        scenario.Incident("Disease_Malaria").Execute();
        scenario.Incident("Disease_SleepingSickness").Execute();
        scenario.Incident("Disease_SensoryMechanites").Execute();

        var pawnWithOrganDecay = pawns.First(p => p.health.hediffSet.hediffs.Any(h => h.def.defName == "OrganDecay"));
        scenario.OpenHistoryRecordTab(pawnWithOrganDecay);
    }
}
