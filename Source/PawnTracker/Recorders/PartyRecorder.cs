using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PartyRecorder : RecorderBase<PartyRecorder.PartyStartedInput>, IRecord<PartyRecorder.PartyInterruptedInput>, IRecord<PartyRecorder.PartyAttendedInput>
{
    public record PartyStartedInput(Pawn Organizer);
    public record PartyJoinedInput(Pawn Organizer, IEnumerable<Pawn> NewJoiners);
    public record PartyAttendedInput(Pawn Organizer, List<Pawn> Partygoers);
    public record PartyInterruptedInput(Pawn Organizer, List<Pawn> Partygoers, PartyFinishedReason Reason, Pawn DeadPawn);

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
                var reason = GetFinishedReason(e.Lord, e.Trigger, partyJob.Organizer, e.Signal, out var deadPawn);
                var partyGoers = e.Lord.ownedPawns.ToList().Concat(partyJob.Organizer /* might be unavailable */).Distinct().ToList();
                
                if (reason == PartyFinishedReason.Timeout)
                    CreateRecord(new PartyAttendedInput(partyJob.Organizer, partyGoers));
                else
                    CreateRecord(new PartyInterruptedInput(partyJob.Organizer, partyGoers, reason, deadPawn));
            }
        });
        // pawn joins party over time, not instantly at the start
        GameEventBus.Subscribe<JoinedLordEvent>(e =>
        {
            if (e.Lord.LordJob is not LordJob_Joinable_Party partyJob)
                return;
            CreateRecord(new PartyJoinedInput(partyJob.Organizer, e.Pawns));
        });
    }

    public override void CreateRecord(PartyStartedInput input)
    {
        var organizer = input.Organizer;
        if (!ShouldRecord(organizer))
            return;

        var recordDef = HistoryRecordDefOf.PartyStarted;
        var desc = recordDef.Description(organizer)
            .AddConstant("isOrganizer", true)
            .Resolve();

        AddRecord(recordDef, organizer, desc);
    }

    public void CreateRecord(PartyJoinedInput input)
    {
        var (organizer, newJoiners) = input;
        var recordDef = HistoryRecordDefOf.PartyJoined;

        foreach (var pawn in newJoiners)
        {
            if (!ShouldRecord(pawn)) continue;
            if (pawn == organizer) continue;

            var desc = recordDef
                .Description(pawn)
                .AddRule("Organizer", organizer)
                .AddConstant("isOrganizer", false)
                .Resolve();

            AddRecord(recordDef, pawn, desc, [organizer]);
        }
    }

    public enum PartyFinishedReason
    {
        Timeout,
        Unknown,
        PawnKilled,
        OrganizerLeft,
        DangerousMap,
    }

    private static PartyFinishedReason GetFinishedReason(Lord lord, Trigger trigger, Pawn organizer, TriggerSignal? signal, out Pawn deadPawn)
    {
        deadPawn = null;

        if (trigger is Trigger_TicksPassed)
            return PartyFinishedReason.Timeout;

        deadPawn = signal?.condition == PawnLostCondition.Killed ? signal.Value.Pawn : null;

        if (trigger is Trigger_PawnKilled)
            return PartyFinishedReason.PawnKilled;
        if (trigger is Trigger_PawnLost || !GatheringsUtility.PawnCanStartOrContinueGathering(organizer))
            return PartyFinishedReason.OrganizerLeft;
        if (trigger is Trigger_TickCondition && !GatheringsUtility.AcceptableGameConditionsToContinueGathering(lord.LordJob.Map))
            return PartyFinishedReason.DangerousMap;

        return PartyFinishedReason.Unknown;
    }

    public void CreateRecord(PartyAttendedInput input)
    {
        var organizer = input.Organizer;
        var partygoers = input.Partygoers;
        var recordDef = HistoryRecordDefOf.PartyFinished;

        foreach (var pawn in partygoers)
        {
            if (!ShouldRecord(pawn))
                continue;
            if (pawn == organizer)
                continue;

            var desc = recordDef
                .Description(pawn)
                .WithOthers(partygoers)
                .AddRule("Organizer", organizer)
                .AddConstant("reason", PartyFinishedReason.Timeout)
                .Resolve();

            AddRecord(recordDef, pawn, desc, [organizer]);
        }
    }

    public void CreateRecord(PartyInterruptedInput input)
    {
        var organizer = input.Organizer;
        var partygoers = input.Partygoers;
        var recordDef = HistoryRecordDefOf.PartyFinished;

        foreach (var pawn in partygoers)
        {
            if (!ShouldRecord(pawn))
                continue;

            var desc = recordDef
                .Description(pawn)
                .AddRule("Organizer", organizer, addSubsymbols: true)
                .AddRule("DeadPawn", input.DeadPawn)
                .AddConstant("reason", input.Reason)
                .Resolve();

            AddRecord(recordDef, pawn, desc, [organizer, input.DeadPawn]);
        }
    }
    
    public static readonly int MinPartyDuration = 1200; // must larger than this value in IsGatheringAboutToEnd()

    public void TestAttended(TestScenario scenario)
    {
        scenario.PartyDuration = MinPartyDuration + 30;
        scenario.SpeedUp();
        
        Expect.Assertions(3);
        var (organizer, lord) = SetupParty(scenario);
        
        scenario.WaitUntil(() => lord.CurLordToil is LordToil_End, () =>
        {
            var partygoers = lord.ownedPawns.ToList();
            var attendees = partygoers.Where(p => p != organizer).ToList();
            
            Expect.ThatAny(attendees).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.PartyJoined,
                Description = "[PAWN] joined [Organizer]'s party.",
                Concerns = [organizer],
            });
            Expect.ThatAny(attendees).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.PartyFinished,
                Description = "[PAWN] attended [Organizer]'s party with [Others].",
                Concerns = [organizer],
            });
        });

        Expect.That(organizer).ToHaveHistoryRecord(HistoryRecordDefOf.PartyStarted, "[PAWN] threw a party for the colony.");
    }

    public Action TestDangerousMap(TestScenario scenario)
    {
        scenario.SpeedUp();
        Expect.Assertions(2);
        var (organizer, lord) = SetupParty(scenario);
        scenario.WaitUntil(() => lord.CurLordToil is LordToil_End, () =>
        {
            var partygoers = lord.ownedPawns.ToList();
            var attendees = partygoers.Where(p => p != organizer).ToList();

            var expected = new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.PartyFinished,
                Description = "[Organizer]'s party was cancelled due to nearby threats.",
            };
            Expect.ThatAny(attendees).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [organizer] }));
            Expect.That(organizer).ToHaveHistoryRecord(expected);
        });

        TickDelayManager.Delay(200, () =>
        {
            scenario.Incident(IncidentDefOf.RaidEnemy).Point(500).Execute();
            scenario.RaidFriendly().Point(500).Execute();
        });

        return () => scenario.SlowDown();
    }

    public Action TestOrganizerLeft(TestScenario scenario)
    {
        scenario.SpeedUp();
        Expect.Assertions(2);
        var (organizer, lord) = SetupParty(scenario);

        scenario.WaitUntil(() => lord.CurLordToil is LordToil_End, () =>
        {
            var partygoers = lord.ownedPawns.ToList();
            var attendees = partygoers.Where(p => p != organizer).ToList();

            var expected = new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.PartyFinished,
                Description = "[Organizer]'s party was cancelled after [He] left.",
            };
            Expect.ThatAny(attendees).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [organizer] }));
            Expect.That(organizer).ToHaveHistoryRecord(expected);
        });

        TickDelayManager.Delay(200, () => organizer.drafter.Drafted = true);

        return () => scenario.SlowDown();
    }

    public Action TestPawnKilled(TestScenario scenario)
    {
        scenario.SpeedUp();
        Expect.Assertions(2);
        var (organizer, lord) = SetupParty(scenario);
        var deadPawn = (Pawn)null;
        scenario.WaitUntil(() => lord.CurLordToil is LordToil_End, () =>
        {
            var partygoers = lord.ownedPawns.ToList();
            var attendees = partygoers.Where(p => p != organizer).ToList();
            
            var expected = new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.PartyFinished,
                Description = "[Organizer]'s party was cancelled after [DeadPawn] died.",
            };
            Expect.ThatAny(attendees).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [organizer, deadPawn] }));
            Expect.That(organizer).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [deadPawn] }));
        });

        scenario.WaitUntil( () => lord.ownedPawns.Count > 2,
            () =>
            {
                deadPawn = lord.ownedPawns.First(p => p != organizer);
                deadPawn.Kill(null);
            });

        return () => scenario.SlowDown();
    }

    private static (Pawn organizer, Lord lord) SetupParty(TestScenario scenario)
    {
        scenario.Map().BuildRoom(8, 8).WithThing(ThingDefOf.PartySpot, 1, Faction.OfPlayer).Execute();
        scenario.Pawn(4).Colonist().Execute();
        var result = scenario.Incident(GatheringDefOf.Party).Execute();
        return (result.Organizers.Single(), result.Lord);
    }
}
