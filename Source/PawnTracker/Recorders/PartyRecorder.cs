using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class PartyRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<LordToilChangeEvent>(e =>
        {
            if (e.Lord.LordJob is not LordJob_Joinable_Party partyJob)
                return;

            if (e.CurrentToil == null && e.NextToil is LordToil_Party)
                HandlePartyStartedEvent(partyJob.Organizer);
            if (e.NextToil is LordToil_End)
                HandlePartyCancelled(e.Lord, e.Trigger, partyJob.Organizer, e.Signal);
        });

        // pawn joins party over time, not instantly at the start
        GameEventBus.Subscribe<JoinedLordEvent>(e =>
        {
            if (e.Lord.LordJob is not LordJob_Joinable_Party partyJob)
                return;
            HandlePartyJoinedEvent(e.Lord, partyJob.Organizer, e.Pawns);
        });
    }

    private void HandlePartyStartedEvent(Pawn organizer)
    {
        if (!ShouldRecord(organizer))
            return;

        var recordDef = HistoryRecordDefOf.JoinedParty;
        var desc = recordDef
            .Description(organizer)
            .AddConstant("isOrganizer", true)
            .Resolve();

        AddRecord(recordDef, organizer, desc);
    }

    enum CancelledReason
    {
        Unknown,
        PawnKilled,
        OrganizerLeft,
        DangerousMap,
    }

    private void HandlePartyCancelled(Lord lord, Trigger trigger, Pawn organizer, TriggerSignal? signal)
    {
        if (trigger is Trigger_TicksPassed)
            return; // party is finished

        var reason = CancelledReason.Unknown;
        var deadPawn = signal?.condition == PawnLostCondition.Killed ? signal?.Pawn : null;

        if (trigger is Trigger_PawnKilled)
            reason = CancelledReason.PawnKilled;
        else if (trigger is Trigger_PawnLost || !GatheringsUtility.PawnCanStartOrContinueGathering(organizer))
            reason = CancelledReason.OrganizerLeft;
        else if (trigger is Trigger_TickCondition && !GatheringsUtility.AcceptableGameConditionsToContinueGathering(lord.LordJob.Map))
            reason = CancelledReason.DangerousMap;

        var recordDef = HistoryRecordDefOf.PartyCancelled;

        foreach (var pawn in lord.ownedPawns)
        {
            if (!ShouldRecord(pawn)) continue;

            var desc = recordDef
                .Description(pawn)
                .AddRule("Organizer", organizer, addSubsymbols: true)
                .AddRule("DeadPawn", deadPawn)
                .AddConstant("reason", reason)
                .Resolve();

            AddRecord(recordDef, pawn, desc, [organizer, deadPawn]);
        }
    }

    private void HandlePartyJoinedEvent(Lord lord, Pawn organizer, IEnumerable<Pawn> newJoiners)
    {
        var recordDef = HistoryRecordDefOf.JoinedParty;
        var joiners = lord.ownedPawns.Where(p => p != organizer).ToList();

        foreach (var pawn in newJoiners)
        {
            if (!ShouldRecord(pawn)) continue;
            if (pawn == organizer) continue;

            var desc = recordDef
                .Description(pawn)
                .WithOthers(joiners)
                .AddRule("Organizer", organizer)
                .AddConstant("isOrganizer", false)
                .Resolve();

            AddRecord(recordDef, pawn, desc, [organizer]);
        }
    }

    public void TestDangerousMap(TestScenario scenario)
    {
        scenario.Pawn(8).Colonist().Execute();
        var res = scenario.Incident(GatheringDefOf.Party).Execute();
        Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;

        TickDelayManager.Delay(400, () =>
        {
            scenario.Incident(IncidentDefOf.RaidEnemy).Point(500).Execute();
            scenario.RaidFriendly().Point(500).Execute();
            Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
        });
    }

    public void TestOrganizerLeft(TestScenario scenario)
    {
        scenario.Pawn(8).Colonist().Execute();
        var organizer = scenario.Incident(GatheringDefOf.Party).Execute().Organizer;
        Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;

        TickDelayManager.Delay(400, () =>
        {
            scenario.Pawn([organizer]).Do(p => p.needs.rest.CurLevel = 0f).Execute();
            Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
        });
    }

    public void TestPawnKilled(TestScenario scenario)
    {
        scenario.Pawn(8).Colonist().Execute();
        var res = scenario.Incident(GatheringDefOf.Party).Execute();
        Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;

        TickDelayManager.Delay(400, () =>
        {
            res.Lord.ownedPawns.FirstOrDefault(p => p != res.Organizer)?.Kill(null);
            Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
        });
    }
}
