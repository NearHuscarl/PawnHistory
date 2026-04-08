using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class DiseaseRecorder : RecorderBase<DiseaseEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<DiseaseEvent>(CreateRecord);
    }

    public override void CreateRecord(DiseaseEvent e)
    {
        var (pawn, group, incidentDef, bodyPart) = e;
        if (!ShouldRecord(pawn))
            return;

        var recordDef = HistoryRecordDefOf.Disease;
        var diseaseHediffDef = incidentDef.diseaseIncident;
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .WithOthers(group.ToList())
            .AddRule("Disease", diseaseHediffDef)
            .AddRule("Part", bodyPart)
            .AddConstant("disease", diseaseHediffDef.defName)
            .Resolve();
        AddRecord(recordDef, pawn, desc);
    }

    [SkipTest]
    public void Test(TestScenario scenario)
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
