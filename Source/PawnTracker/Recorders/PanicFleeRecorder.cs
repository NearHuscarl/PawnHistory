using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PanicFleeRecorder : RecorderBase<PanicFleeRecorder.Input>
{
    public record Input(List<Pawn> Pawns, Faction Faction);

    public override void Register()
    {
        GameEventBus.Subscribe<LordToilChangeEvent>(e =>
        {
            if (e.NextToil is not LordToil_PanicFlee)
                return;

            var pawns = e.Lord.ownedPawns.Where(p => !p.Dead).ToList();
            CreateRecord(new Input(pawns, e.Lord.faction));
        });
    }

    public override void CreateRecord(Input input)
    {
        var (pawns, faction) = input;
        var recordDef = HistoryRecordDefOf.PanicFlee;

        foreach (var pawn in pawns)
        {
            if (!ShouldRecord(pawn))
                continue;

            var desc = recordDef.Description(pawn)
                .WithOthers(pawns)
                .AddRule("Faction", faction)
                .Resolve();

            AddRecord(recordDef, pawn, desc);
        }
    }

    public void Test(TestScenario scenario)
    {
        var raiders = scenario.Incident(IncidentDefOf.RaidEnemy)
            .Faction(Faction.OfHostile)
            .Point(700)
            .RaidArrivalMode(PawnsArrivalModeDefOf.EdgeWalkIn)
            .Execute();

        for (var i = 0; i < raiders.Count - 2; i++)
        {
            if (raiders[i].lord?.CurLordToil is LordToil_PanicFlee)
                break;
            raiders[i].Kill(null);
        }

        var fleeingRaiders = raiders.Where(p => !p.Dead).ToList();

        Expect.ThatAll(fleeingRaiders).ToHaveHistoryRecord(HistoryRecordDefOf.PanicFlee, "[PAWN] from [Faction] broke and fled in panic.");
    }
}
