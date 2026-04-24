using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PartyRecorder : RecorderBase<PartyRecorder.PartyStartedInput>, IRecord<PartyRecorder.PartyJoinedInput>, IRecord<PartyRecorder.PartyCancelledInput>
{
    public record PartyStartedInput(Pawn Organizer);
    public record PartyJoinedInput(Pawn Organizer, List<Pawn> Partygoers, IEnumerable<Pawn> NewJoiners);
    public record PartyCancelledInput(Pawn Organizer, List<Pawn> Partygoers, CancelledReason Reason, Pawn DeadPawn);

    public override void Register()
    {
        GameEventBus.Subscribe<LordToilChangeEvent>(e =>
        {
            if (e.Lord.LordJob is not LordJob_Joinable_Party partyJob)
                return;

            if (e.CurrentToil == null && e.NextToil is LordToil_Party)
                CreateRecord(new PartyStartedInput(partyJob.Organizer));
            if (e.NextToil is LordToil_End)
            {
                var cancelledReason = GetCancelledReason(e.Lord, e.Trigger, partyJob.Organizer, e.Signal, out var deadPawn);
                CreateRecord(new PartyCancelledInput(partyJob.Organizer, e.Lord.ownedPawns, cancelledReason, deadPawn));
            }
        });

        // pawn joins party over time, not instantly at the start
        GameEventBus.Subscribe<JoinedLordEvent>(e =>
        {
            if (e.Lord.LordJob is not LordJob_Joinable_Party partyJob)
                return;
            CreateRecord(new PartyJoinedInput(partyJob.Organizer, e.Lord.ownedPawns.ToList(), e.Pawns));
        });
    }

    public override void CreateRecord(PartyStartedInput input)
    {
        var organizer = input.Organizer;
        if (!ShouldRecord(organizer))
            return;

        var recordDef = HistoryRecordDefOf.JoinedParty;
        var desc = recordDef
            .Description(organizer)
            .AddConstant("isOrganizer", true)
            .Resolve();

        AddRecord(recordDef, organizer, desc);
    }

    public void CreateRecord(PartyJoinedInput input)
    {
        var (organizer, partygoers, newJoiners) = input;
        var recordDef = HistoryRecordDefOf.JoinedParty;

        foreach (var pawn in newJoiners)
        {
            if (!ShouldRecord(pawn)) continue;
            if (pawn == organizer) continue;

            var desc = recordDef
                .Description(pawn)
                .WithOthers(partygoers)
                .AddRule("Organizer", organizer)
                .AddConstant("isOrganizer", false)
                .Resolve();

            AddRecord(recordDef, pawn, desc, [organizer]);
        }
    }

    public enum CancelledReason
    {
        Unknown,
        Timeout,
        PawnKilled,
        OrganizerLeft,
        DangerousMap,
    }

    private static CancelledReason GetCancelledReason(Lord lord, Trigger trigger, Pawn organizer, TriggerSignal? signal, out Pawn deadPawn)
    {
        deadPawn = null;

        if (trigger is Trigger_TicksPassed)
            return CancelledReason.Timeout; // party is finished

        var reason = CancelledReason.Unknown;
        deadPawn = signal?.condition == PawnLostCondition.Killed ? signal.Value.Pawn : null;

        if (trigger is Trigger_PawnKilled)
            reason = CancelledReason.PawnKilled;
        else if (trigger is Trigger_PawnLost || !GatheringsUtility.PawnCanStartOrContinueGathering(organizer))
            reason = CancelledReason.OrganizerLeft;
        else if (trigger is Trigger_TickCondition && !GatheringsUtility.AcceptableGameConditionsToContinueGathering(lord.LordJob.Map))
            reason = CancelledReason.DangerousMap;

        return reason;
    }

    public void CreateRecord(PartyCancelledInput input)
    {
        var (organizer, partygoers, reason, deadPawn) = input;
        var recordDef = HistoryRecordDefOf.PartyCancelled;

        if (reason == CancelledReason.Timeout)
            return;

        foreach (var pawn in partygoers)
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

    [SkipTest]
    public void TestDangerousMap(TestScenario scenario)
    {
        scenario.SpeedUp();
        scenario.Map().BuildRoom(5, 5).WithThing(ThingDefOf.PartySpot, 1, Faction.OfPlayer).Execute();
        scenario.Pawn(8).Colonist().Execute();
        scenario.Incident(GatheringDefOf.Party).Execute();

        TickDelayManager.Delay(400, () =>
        {
            scenario.Incident(IncidentDefOf.RaidEnemy).Point(500).Execute();
            scenario.RaidFriendly().Point(500).Execute();
            scenario.SlowDown();
        });
    }

    [SkipTest]
    public void TestOrganizerLeft(TestScenario scenario)
    {
        scenario.SpeedUp();
        scenario.Map().BuildRoom(5, 5).WithThing(ThingDefOf.PartySpot, 1, Faction.OfPlayer).Execute();
        scenario.Pawn(8).Colonist().Execute();
        var organizer = scenario.Incident(GatheringDefOf.Party).Execute().Organizers.Single();

        TickDelayManager.Delay(400, () =>
        {
            scenario.Pawn(organizer).Do(p => p.needs.rest.CurLevel = 0f).Execute();
            scenario.SlowDown();
        });
    }

    [SkipTest]
    public void TestPawnKilled(TestScenario scenario)
    {
        scenario.SpeedUp();
        scenario.Map().BuildRoom(5, 5).WithThing(ThingDefOf.PartySpot, 1, Faction.OfPlayer).Execute();
        scenario.Pawn(8).Colonist().Execute();
        var res = scenario.Incident(GatheringDefOf.Party).Execute();

        TickDelayManager.Delay(400, () =>
        {
            res.Lord.ownedPawns.FirstOrDefault(p => res.Organizers.Contains(p))?.Kill(null);
            scenario.SlowDown();
        });
    }
}
