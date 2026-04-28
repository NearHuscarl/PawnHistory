using PawnHistory.Source.PawnTracker.Test;
using PawnHistory.Source.PawnTracker.Events;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class WeddingRecorder : RecorderBase<WeddingRecorder.WeddingStartedInput>, IRecord<WeddingRecorder.WeddingJoinedInput>, IRecord<WeddingRecorder.WeddingCancelledInput>
{
    public record WeddingStartedInput(Pawn FirstPawn, Pawn SecondPawn);
    public record WeddingJoinedInput(Pawn FirstPawn, Pawn SecondPawn, List<Pawn> WeddingGoers, IEnumerable<Pawn> NewJoiners);
    public record WeddingCancelledInput(Pawn FirstPawn, Pawn SecondPawn, List<Pawn> WeddingGoers, CancelledReason Reason, Pawn DeadPawn);

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
                var cancelledReason = GetCancelledReason(e.Lord, e.Trigger, e.Signal, out var deadPawn);
                var weddingGoers = GetWeddingGoers(e.Lord, weddingJob.firstPawn, weddingJob.secondPawn);
                CreateRecord(new WeddingCancelledInput(weddingJob.firstPawn, weddingJob.secondPawn, weddingGoers, cancelledReason, deadPawn));
            }
        });

        // pawn joins wedding over time, not instantly at the start
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
        var recordDef = HistoryRecordDefOf.WeddingJoined;
        var couple = new [] { firstPawn, secondPawn };

        foreach (var pawn in couple)
        {
            if (!ShouldRecord(pawn))
                continue;
            
            var otherPawn = pawn == firstPawn ? secondPawn : firstPawn;
            var desc = recordDef
                .Description(pawn)
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
                .WithOthers(weddingGoers.Except(couple).ToList())
                .AddRule("Pawn1", firstPawn)
                .AddRule("Pawn2", secondPawn)
                .AddConstant("isSpectator", true)
                .Resolve();

            AddRecord(recordDef, pawn, desc, couple);
        }
    }
    
    public enum CancelledReason
    {
        Unknown,
        Success,
        PawnKilled,
        DangerousMap,
    }
    private static CancelledReason GetCancelledReason(Lord lord, Trigger trigger, TriggerSignal? signal, out Pawn deadPawn)
    {
        var reason = CancelledReason.Unknown;
        deadPawn = signal?.condition == PawnLostCondition.Killed ? signal.Value.Pawn : null;

        if (lord.LordJob is LordJob_Joinable_MarriageCeremony job && job.firstPawn.relations.DirectRelationExists(PawnRelationDefOf.Spouse, job.secondPawn))
            reason =  CancelledReason.Success;
        else if (trigger is Trigger_PawnKilled)
            reason = CancelledReason.PawnKilled;
        else if (trigger is Trigger_TickCondition && !GatheringsUtility.AcceptableGameConditionsToContinueGathering(lord.LordJob.Map))
            reason = CancelledReason.DangerousMap;
        
        return reason;
    }
    
    public void CreateRecord(WeddingCancelledInput input)
    {
        var recordDef = HistoryRecordDefOf.WeddingCancelled;
        var couple = new[] { input.FirstPawn, input.SecondPawn };
        
        if (input.Reason == CancelledReason.Success)
            return;

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

    public void Test(TestScenario scenario)
    {
        scenario.SpeedUp();
        
        var (couple, spectators) = SetupWedding(scenario);

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.WeddingJoined,
            Description = "[PAWN] and [Pawn2] began their wedding ceremony.",
        };
        Expect.That(couple[0]).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [couple[1]] }));
        Expect.That(couple[1]).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [couple[0]] }));
        Expect.ThatAll(spectators).Eventually().ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.WeddingJoined,
            Description = "[PAWN] attended [Pawn1] and [Pawn2]'s wedding ceremony",
            Concerns = couple.Cast<Thing>().ToList(),
        });
    }

    public Action TestDead(TestScenario scenario)
    {
        scenario.SpeedUp();
        var (couple, spectators) = SetupWedding(scenario);
        var victim = spectators.First();

        TickDelayManager.Delay(100, () => victim.Kill(null));

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.WeddingCancelled,
            Description = "[Pawn1] and [Pawn2]'s wedding was called off after [DeadPawn] died.",
        };
        Expect.That(couple[0]).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [victim, couple[1]] }));
        Expect.That(couple[1]).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [victim, couple[0]] }));
        Expect.ThatAny(spectators).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [victim, ..couple] }));

        return () => scenario.SlowDown();
    }

    public Action TestDangerousMap(TestScenario scenario)
    {
        scenario.SpeedUp();
        var (couple, spectators) = SetupWedding(scenario);

        TickDelayManager.Delay(100, () =>
        {
            scenario.Incident(IncidentDefOf.RaidEnemy).Point(500).Execute();
            scenario.RaidFriendly().Point(500).Execute();
        });
        
        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.WeddingCancelled,
            Description = "[Pawn1] and [Pawn2]'s wedding was called off due to nearby threats.",
        };
        Expect.That(couple[0]).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [couple[1]] }));
        Expect.That(couple[1]).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [couple[0]] }));
        Expect.ThatAny(spectators).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = couple.Cast<Thing>().ToList() }));

        return () => scenario.SlowDown();
    }

    public static (List<Pawn> couple, List<Pawn> spectators) SetupWedding(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(9, 9)
            .WithThing(ThingDefOf.MarriageSpot, 1)
            .Execute();

        var pawn1 = scenario.Pawn().Colonist().CreateSingle();
        var pawn2 = scenario.Pawn().Colonist().SetRelation(pawn1, PawnRelationDefOf.Fiance).CreateSingle();
        var spectators = scenario.Pawn(3).Colonist().FullHeal().Execute();

        scenario.Incident(GatheringDefOf.MarriageCeremony).Execute();
        return ([pawn1, pawn2], spectators);
    }
}
