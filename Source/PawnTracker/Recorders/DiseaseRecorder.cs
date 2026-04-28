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

    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(8).Colonist().Execute();
        scenario.Incident(Extra.IncidentDefOf.Disease_Malaria).Execute();
        scenario.Incident(Extra.IncidentDefOf.Disease_SleepingSickness).Execute();
        scenario.Incident(Extra.IncidentDefOf.Disease_SensoryMechanites).Execute();
        scenario.Incident(Extra.IncidentDefOf.Disease_OrganDecay).Execute();

        Expect.ThatAny(pawns).ToHaveHistoryRecord(HistoryRecordDefOf.Disease, "[PAWN] got sick from malaria along with [n] [Others].");
        Expect.ThatAny(pawns).ToHaveHistoryRecord(HistoryRecordDefOf.Disease, "[PAWN] got sick from sleeping sickness along with [n] [Others].");
        Expect.ThatAny(pawns).ToHaveHistoryRecord(HistoryRecordDefOf.Disease, "[PAWN] got sick from sensory mechanites along with [n] [Others].");
        Expect.ThatAny(pawns).ToHaveHistoryRecord(HistoryRecordDefOf.Disease, "[PAWN] developed a flesh-eating infection known as organ decay in [His] [Part].");
    }
}
