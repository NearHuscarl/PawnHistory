using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class ManInBlackJoinRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<WandererJoinEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn))
                return;
            if (e.IncidentDef.defName != "StrangerInBlackJoin")
                return;

            HandleManInBlackJoinEvent(e);
        });
    }

    private void HandleManInBlackJoinEvent(WandererJoinEvent e)
    {
        var recordDef = HistoryRecordDefOf.ManInBlackJoin;
        var rel = e.Pawn.relations.PotentiallyRelatedPawns
            .Where(p => ShouldRecord(p) && p.IsColonist && !p.wasLeftBehindStartingPawn)
            .Select(p => new { Pawn = p, Relation = p.GetMostImportantRelation(e.Pawn) })
            .Where(x => x.Relation != null)
            .OrderByDescending(x => x.Relation.importance)
            .FirstOrDefault();

        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddRule("Other", rel?.Pawn, addSubsymbols: true)
            .AddRule("Relation", rel?.Relation.GetGenderSpecificLabel(e.Pawn))
            .AddConstant("hasRelation", rel != null)
            .Resolve();
        AddRecord(recordDef, e.Pawn, desc, [rel?.Pawn]);
    }

    public override void Test(TestScenario scenario)
    {
        scenario.Incident("StrangerInBlackJoin").Execute();
    }
}
