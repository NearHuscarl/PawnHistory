using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class ManInBlackJoinedRecorder : RecorderBase<WandererJoinedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<WandererJoinedEvent>(e =>
        {
            if (e.IncidentDef?.defName != "StrangerInBlackJoin")
                return;

            CreateRecord(e);
        });
    }

    public override void CreateRecord(WandererJoinedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var pawn = e.Pawn;
        var recordDef = HistoryRecordDefOf.ManInBlackJoin;
        var relative = PawnRelationUtility.GetMostImportantColonyRelative(pawn);
        var relation = relative?.GetMostImportantRelation(pawn)?.GetGenderSpecificLabel(pawn);
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .AddRule("Other", relative, addSubsymbols: true)
            .AddRule("Relation", relation)
            .AddConstant("hasRelation", relative != null)
            .Resolve();
        AddRecord(recordDef, pawn, desc, [relative]);
    }

    [SkipTest]
    public void Test(TestScenario scenario)
    {
        scenario.Incident(DefLookup.Incident.StrangerInBlackJoin).Execute();
    }
}
