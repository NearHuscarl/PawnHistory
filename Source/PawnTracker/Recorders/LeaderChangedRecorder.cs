using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class LeaderChangedRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<LeaderChangedEvent>(e =>
        {
            if (!ShouldRecord(e.NewLeader))
                return;

            HandleLeaderChangedEvent(e);
        });
    }

    private void HandleLeaderChangedEvent(LeaderChangedEvent e)
    {
        var recordDef = HistoryRecordDefOf.LeaderChanged;
        var desc = recordDef.Description(e.NewLeader)
            .AddRule("OldLeader", e.OldLeader)
            .AddRule("Faction", e.NewLeader.Faction, addSubsymbols: true)
            .AddConstant("reason", e.Reason)
            .Resolve();

        AddRecord(recordDef, e.NewLeader, desc, [e.OldLeader]);
    }

    public void TestLost(TestScenario scenario)
    {
        var oldLeader = scenario.Pawn()
            .FactionLeader(Faction.OfPirates)
            .CreateSingle();
        
        oldLeader.SetFaction(Faction.OfPlayer);

        var newLeader = scenario.Pawn()
            .FactionLeader(Faction.OfPirates)
            .CreateSingle();

        Expect.That(newLeader).ToHaveHistoryRecord("[PAWN] became the new [Faction_leaderTitle] of [Faction] after [OldLeader] went missing.");
    }

    public void TestDeath(TestScenario scenario)
    {
        var oldLeader = scenario.Pawn()
            .FactionLeader(Faction.OfPirates)
            .CreateSingle();

        HealthUtility.DamageUntilDead(oldLeader);

        var newLeader = scenario.Pawn()
            .FactionLeader(Faction.OfPirates)
            .CreateSingle();

        Expect.That(newLeader).ToHaveHistoryRecord("[PAWN] became the new [Faction_leaderTitle] of [Faction] after [OldLeader] died.");
    }
}