using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RaidFriendlyRecorder : RecorderBase<RaidStartedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<RaidStartedEvent>(CreateRecord);
    }

    public override void CreateRecord(RaidStartedEvent input)
    {
        var (pawns, faction, raidStrategy, raidArrivalMode, isFriendly, quest) = input;

        if (!isFriendly)
            return;
        
        pawns = pawns.Where(ShouldRecord).ToList();
        var recordDef = HistoryRecordDefOf.RaidFriendly;
        var hostileFaction = pawns[0].MapHeld.lordManager.lords
            .FirstOrDefault(l => l.faction != null && l.faction.HostileTo(faction))
            ?.faction;

        foreach (var pawn in pawns)
        {
            var desc = recordDef.Description(pawn)
                .WithOthers(pawns)
                .AddRule("Faction", faction)
                .AddRule("HostileFaction", hostileFaction)
                .AddConstant("enemyHasFaction", hostileFaction != null) // not manhunter/insect
                .Resolve();

            AddRecord(recordDef, pawn, desc);
        }
    }

    public void TestFriendly(TestScenario scenario)
    {
        scenario.Incident(IncidentDefOf.RaidEnemy).Point(170).Execute();
        var pawns = scenario.RaidFriendly().Point(170).Execute();
        Expect.ThatAll(pawns).ToHaveHistoryRecord(HistoryRecordDefOf.RaidFriendly, "[PAWN] and [Others] from [Faction] came to aid the colony against [HostileFaction].");
    }
    
    [DebugValues(70, 100, 140, 500)]
    public void TestFriendlyWithParams(TestScenario scenario, int point)
    {
        scenario.Incident(IncidentDefOf.RaidEnemy).Point(point).Execute();
        scenario.RaidFriendly().Point(point).Execute();
    }
}
