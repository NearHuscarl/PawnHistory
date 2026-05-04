using PawnHistory.Source.PawnTracker.Test;
using PawnHistory.Source.PawnTracker.Events;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class WeddingRecorder : RecorderBase<WeddingRecorder.WeddingStartedInput>, IRecord<WeddingRecorder.WeddingJoinedInput>, IRecord<WeddingRecorder.WeddingInterruptedInput>, IRecord<WeddingRecorder.WeddingAttendedEvent>
{
    public record WeddingStartedInput(Pawn FirstPawn, Pawn SecondPawn);
    public record WeddingJoinedInput(Pawn FirstPawn, Pawn SecondPawn, List<Pawn> WeddingGoers, IEnumerable<Pawn> NewJoiners);
    public record WeddingAttendedEvent(Pawn FirstPawn, Pawn SecondPawn, List<Pawn> WeddingGoers);
    public record WeddingInterruptedInput(Pawn FirstPawn, Pawn SecondPawn, List<Pawn> WeddingGoers, WeddingFinishedReason Reason, Pawn DeadPawn);

    public override void Register()
    {
        GameEventBus.Subscribe<LordToilChangeEvent>(e =>
        {
            if (e.Lord.LordJob is not LordJob_Joinable_MarriageCeremony weddingJob)
                return;

            if (e.CurrentToil == null && e.NextToil is LordToil_Party)
                CreateRecord(new WeddingStartedInput(weddingJob.firstPawn, weddingJob.secondPawn));
            if (e.NextToil is LordToil_End)
            {
                var reason = GetFinishedReason(e.Lord, e.Trigger, e.Signal, out var deadPawn);
                var weddingGoers = GetWeddingGoers(e.Lord, weddingJob.firstPawn, weddingJob.secondPawn);

                if (reason == WeddingFinishedReason.Timeout)
                    CreateRecord(new WeddingAttendedEvent(weddingJob.firstPawn, weddingJob.secondPawn, weddingGoers));
                else
                    CreateRecord(new WeddingInterruptedInput(weddingJob.firstPawn, weddingJob.secondPawn, weddingGoers, reason, deadPawn));
            }
        });

        GameEventBus.Subscribe<JoinedLordEvent>(e =>
        {
            if (e.Lord.LordJob is not LordJob_Joinable_MarriageCeremony weddingJob)
                return;
            var weddingGoers = GetWeddingGoers(e.Lord, weddingJob.firstPawn, weddingJob.secondPawn);
            CreateRecord(new WeddingJoinedInput(weddingJob.firstPawn, weddingJob.secondPawn, weddingGoers, e.Pawns));
        });
    }

    // one of the couple is missing from lord.ownedPawns
    private static List<Pawn> GetWeddingGoers(Lord lord, Pawn pawn1, Pawn pawn2) => lord.ownedPawns.Concat([pawn1, pawn2]).Distinct().ToList();

    public override void CreateRecord(WeddingStartedInput input)
    {
        var (firstPawn, secondPawn) = input;
        var recordDef = HistoryRecordDefOf.WeddingStarted;
        var couple = new [] { firstPawn, secondPawn };

        foreach (var pawn in couple)
        {
            if (!ShouldRecord(pawn))
                continue;
            
            var otherPawn = pawn == firstPawn ? secondPawn : firstPawn;
            var desc = recordDef
                .Description(pawn, "Pawn1")
                .AddRule("Pawn2", otherPawn)
                .AddConstant("isSpectator", false)
                .Resolve();

            AddRecord(recordDef, pawn, desc, [otherPawn]);
        }
    }

    public void CreateRecord(WeddingJoinedInput input)
    {
        var recordDef = HistoryRecordDefOf.WeddingJoined;
        var (firstPawn, secondPawn, weddingGoers, newJoiners) = input;
        var couple = new HashSet<Pawn> { firstPawn, secondPawn };

        foreach (var pawn in newJoiners)
        {
            if (!ShouldRecord(pawn))
                continue;
            if (couple.Contains(pawn))
                continue;

            var desc = recordDef
                .Description(pawn)
                .AddRule("Pawn1", firstPawn)
                .AddRule("Pawn2", secondPawn)
                .AddConstant("isSpectator", true)
                .Resolve();

            AddRecord(recordDef, pawn, desc, couple);
        }
    }
    
    public void CreateRecord(WeddingAttendedEvent input)
    {
        var recordDef = HistoryRecordDefOf.WeddingFinished;
        var (firstPawn, secondPawn, weddingGoers) = input;
        var couple = new[] { firstPawn, secondPawn };
        var spectators = weddingGoers.Except(couple).ToList();

        foreach (var pawn in spectators)
        {
            if (!ShouldRecord(pawn))
                continue;
            if (couple.Contains(pawn))
                continue;

            var desc = recordDef
                .Description(pawn)
                .WithOthers(spectators)
                .AddRule("Pawn1", firstPawn)
                .AddRule("Pawn2", secondPawn)
                .AddConstant("reason", WeddingFinishedReason.Timeout)
                .Resolve();

            AddRecord(recordDef, pawn, desc, couple);
        }
    }
    
    public enum WeddingFinishedReason
    {
        Timeout,
        Unknown,
        PawnKilled,
        DangerousMap,
    }

    private static WeddingFinishedReason GetFinishedReason(Lord lord, Trigger trigger, TriggerSignal? signal, out Pawn deadPawn)
    {
        var reason = WeddingFinishedReason.Unknown;
        deadPawn = signal?.condition == PawnLostCondition.Killed ? signal.Value.Pawn : null;

        if (lord.LordJob is LordJob_Joinable_MarriageCeremony job && job.firstPawn.relations.DirectRelationExists(PawnRelationDefOf.Spouse, job.secondPawn))
            reason =  WeddingFinishedReason.Timeout;
        else if (trigger is Trigger_PawnKilled)
            reason = WeddingFinishedReason.PawnKilled;
        else if (trigger is Trigger_TickCondition && !GatheringsUtility.AcceptableGameConditionsToContinueGathering(lord.LordJob.Map))
            reason = WeddingFinishedReason.DangerousMap;
        
        return reason;
    }

    public void CreateRecord(WeddingInterruptedInput input)
    {
        var recordDef = HistoryRecordDefOf.WeddingFinished;
        var couple = new[] { input.FirstPawn, input.SecondPawn };

        foreach (var pawn in input.WeddingGoers)
        {
            if (!ShouldRecord(pawn))
                continue;

            var desc = recordDef
                .Description(pawn)
                .AddRule("Pawn1", input.FirstPawn)
                .AddRule("Pawn2", input.SecondPawn)
                .AddRule("DeadPawn", input.DeadPawn)
                .AddConstant("reason", input.Reason)
                .Resolve();

            AddRecord(recordDef, pawn, desc, [..couple, input.DeadPawn]);
        }
    }

    public void TestAttended(TestScenario scenario)
    {
        scenario.PartyDuration = PartyRecorder.MinPartyDuration + 30;
        Expect.Assertions(4);
        
        var (couple, spectators, lord) = SetupWedding(scenario);
        var transition1 = lord.Graph.transitions.First(t => t.target is LordToil_MarriageCeremony);
        transition1.AddTrigger(new Trigger_TicksPassed(0));
        MarriageCeremonyUtility.Married(couple[0], couple[1]);
        var transition3 = lord.Graph.transitions.First(t => t.target is LordToil_End && t.sources.OfType<LordToil_Party>().Any());
        transition3.AddTrigger(new Trigger_TicksPassed(0));
        
        scenario.WaitUntil(() => lord.CurLordToil is LordToil_End, () =>
        {
            Expect.ThatAny(spectators).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.WeddingJoined,
                Description = "[PAWN] joined [Pawn1] and [Pawn2]'s wedding ceremony.",
                Concerns = couple.Cast<Thing>().ToList(),
            });
            Expect.ThatAny(spectators).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.WeddingFinished,
                Description = "[PAWN] attended [Pawn1] and [Pawn2]'s wedding ceremony with [Others].",
                Concerns = couple.Cast<Thing>().ToList(),
            });
        });

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.WeddingStarted,
            Description = "[PAWN] and [Pawn2] began their wedding ceremony.",
        };
        Expect.That(couple[0]).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [couple[1]] }));
        Expect.That(couple[1]).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [couple[0]] }));
    }

    public Action TestDead(TestScenario scenario)
    {
        scenario.SpeedUp();
        Expect.Assertions(3);
        var (couple, spectators, lord) = SetupWedding(scenario);
        var victim = spectators.First();
        scenario.WaitUntil(() => lord.CurLordToil is LordToil_End, () =>
        {
            var expected = new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.WeddingFinished,
                Description = "[Pawn1] and [Pawn2]'s wedding was called off after [DeadPawn] died.",
            };
            Expect.That(couple[0]).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [victim, couple[1]] }));
            Expect.That(couple[1]).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [victim, couple[0]] }));
            Expect.ThatAny(spectators).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [victim, ..couple] }));
        });

        TickDelayManager.Delay(100, () => victim.Kill(null));

        return () => scenario.SlowDown();
    }

    public Action TestDangerousMap(TestScenario scenario)
    {
        scenario.SpeedUp();
        Expect.Assertions(3);
        var (couple, spectators, lord) = SetupWedding(scenario);
        scenario.WaitUntil(() => lord.CurLordToil is LordToil_End, () =>
        {
            var expected = new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.WeddingFinished,
                Description = "[Pawn1] and [Pawn2]'s wedding was called off due to nearby threats.",
            };
            Expect.That(couple[0]).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [couple[1]] }));
            Expect.That(couple[1]).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [couple[0]] }));
            Expect.ThatAny(spectators).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = couple.Cast<Thing>().ToList() }));
        });

        TickDelayManager.Delay(100, () =>
        {
            scenario.Incident(IncidentDefOf.RaidEnemy).Point(500).Execute();
            scenario.RaidFriendly().Point(500).Execute();
        });

        return () => scenario.SlowDown();
    }

    public static (List<Pawn> couple, List<Pawn> spectators, Lord lord) SetupWedding(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(9, 9)
            .WithThing(ThingDefOf.MarriageSpot, 1)
            .Execute();

        var pawn1 = scenario.Pawn().Colonist().FullHeal().CreateSingle();
        var pawn2 = scenario.Pawn().Colonist().FullHeal().SetRelation(pawn1, PawnRelationDefOf.Fiance).CreateSingle();
        var spectators = scenario.Pawn(3).Colonist().FullHeal().Execute();

        var result = scenario.Incident(GatheringDefOf.MarriageCeremony).Execute();
        return ([pawn1, pawn2], spectators, result.Lord);
    }
}
