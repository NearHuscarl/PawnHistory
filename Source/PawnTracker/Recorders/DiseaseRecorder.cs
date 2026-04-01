using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using System.Linq;

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
        var desc = recordDef.Description(e.Pawn)
            .WithOthers(e.Group.ToList())
            .AddRule("Disease", e.IncidentDef.diseaseIncident)
            .Resolve();
        AddRecord(recordDef, e.Pawn, desc);
    }

    public override void Test(TestScenario scenario)
    {
        scenario.Pawn(15).Colonist().Execute();
        scenario.Incident("Disease_Malaria").Execute();
        scenario.Incident("Disease_SleepingSickness").Execute();
        scenario.Incident("Disease_SensoryMechanites").Execute();
    }
}
