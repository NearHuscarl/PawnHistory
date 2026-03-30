using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class ManInBlackJoinRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<WandererJoinedEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn))
                return;
            if (e.IncidentDef?.defName != "StrangerInBlackJoin")
                return;

            HandleManInBlackJoinEvent(e);
        });
    }

    private void HandleManInBlackJoinEvent(WandererJoinedEvent e)
    {
        var recordDef = HistoryRecordDefOf.ManInBlackJoin;
        var relative = PawnRelationUtility.GetMostImportantColonyRelative(e.Pawn);
        var relation = relative?.GetMostImportantRelation(e.Pawn)?.GetGenderSpecificLabel(e.Pawn);
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddRule("Other", relative, addSubsymbols: true)
            .AddRule("Relation", relation)
            .AddConstant("hasRelation", relative != null)
            .Resolve();
        AddRecord(recordDef, e.Pawn, desc, [relative]);
    }

    public override void Test(TestScenario scenario)
    {
        scenario.Incident("StrangerInBlackJoin").Execute();
    }
}
