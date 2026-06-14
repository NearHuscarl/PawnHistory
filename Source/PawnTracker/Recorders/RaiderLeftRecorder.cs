using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RaiderLeftRecorder : RecorderBase<RaiderLeftRecorder.Input>
{
    public record Input(List<Pawn> Pawns, Faction Faction, RaiderLeftReason Reason);

    public enum RaiderLeftReason
    {
        Unknown,
        FactionNoLongerHostile,
        GivenUp,
        Satisfied,
    }

    public override void Register()
    {
        GameEventBus.Subscribe<LordToilChangeEvent>(e =>
        {
            if (!TryGetReason(e, out var reason))
                return;

            CreateRecord(new Input(e.Lord.ownedPawns, e.Lord.faction, reason));
        });
    }

    public override void CreateRecord(Input input)
    {
        var (pawns, faction, reason) = input;
        var recordDef = HistoryRecordDefOf.RaidersLeft;

        foreach (var pawn in pawns)
        {
            if (!ShouldRecord(pawn))
                continue;

            var desc = recordDef.Description(pawn)
                .IncludePawnGrammar()
                .WithOthers(pawns)
                .WithPlayerFaction()
                .AddRule("Faction", faction)
                .AddConstant("reason", reason)
                .Resolve();

            AddRecord(recordDef, pawn, desc);
        }
    }

    private static bool TryGetReason(LordToilChangeEvent e, out RaiderLeftReason reason)
    {
        reason = RaiderLeftReason.Unknown;

        if (e.NextToil is not LordToil_ExitMap)
            return false;
        if (!IsRaidLordJob(e.Lord.LordJob))
            return false;

        if (e.Trigger is Trigger_BecameNonHostileToPlayer)
        {
            reason = RaiderLeftReason.FactionNoLongerHostile;
            return true;
        }
        if (e.Trigger is Trigger_TicksPassed)
        {
            reason = RaiderLeftReason.GivenUp;
            return true;
        }
        if (e.Trigger is Trigger_FractionColonyDamageTaken)
        {
            reason = RaiderLeftReason.Satisfied;
            return true;
        }

        return false;
    }

    private static bool IsRaidLordJob(LordJob lordJob)
    {
        // look for RaidStrategyWorker_*.MakeLordJob() to see all available LordJob_[Raid]s
        // Devtool actions > T: DisplayLordGraph
        return lordJob is LordJob_AssaultColony
            or LordJob_AssistColony
            or LordJob_AssaultThings
            or LordJob_StageThenAttack
            or LordJob_Siege
            or LordJob_SleepThenAssaultColony;
    }

    private static (Transition transition, TTrigger trigger) FindExitMapTrigger<TTrigger>(Lord lord) where TTrigger : Trigger
    {
        foreach (var transition in lord.Graph.transitions)
        {
            if (!transition.sources.Contains(lord.CurLordToil) || transition.target is not LordToil_ExitMap)
                continue;

            var trigger = transition.triggers.OfType<TTrigger>().FirstOrDefault();
            if (trigger != null)
                return (transition, trigger);
        }

        throw new InvalidOperationException($"Could not find raid exit-map trigger {typeof(TTrigger).Name}.");
    }

    private static (List<Pawn> raiders, Lord lord) SetupRaid(TestScenario scenario)
    {
        var raiders = scenario.Incident(IncidentDefOf.RaidEnemy)
            .Faction(Faction.OfHostile)
            .Point(500)
            .RaidStrategy(RaidStrategyDefOf.ImmediateAttack) // LordJob_StageThenAttack doesn't have ExitMap in some cases
            .RaidNeverFlee()
            .Execute();

        scenario.Map().BuildRoom(8, 8).Execute();
        scenario.Pawn(3).Colonist().Position(scenario.LastRoomRect.CenterCell).Execute();

        var lord = raiders.First().GetLord();

        return (raiders, lord);
    }

    public void TestNoLongerHostile(TestScenario scenario)
    {
        var (raiders, _) = SetupRaid(scenario);

        Faction.OfPlayer.TryAffectGoodwillWith(raiders[0].Faction, 200);

        Expect.ThatAll(raiders).Eventually().ToHaveHistoryRecord(
            HistoryRecordDefOf.RaidersLeft, "[PAWN] from [Faction] began to leave after [His] faction was no longer hostile to the colony.");
    }

    public void TestGivenUp(TestScenario scenario)
    {
        scenario.SpeedUp();
        var (raiders, lord) = SetupRaid(scenario);

        MockTriggerTime(lord);

        Expect.ThatAll(raiders).Eventually().ToHaveHistoryRecord(HistoryRecordDefOf.RaidersLeft, "[PAWN] from [Faction] gave up and began to leave.");
    }

    public void TestSatisfied(TestScenario scenario)
    {
        scenario.SpeedUp();
        var (raiders, lord) = SetupRaid(scenario);

        MockDamageTaken(lord);

        Expect.ThatAll(raiders).Eventually().ToHaveHistoryRecord(HistoryRecordDefOf.RaidersLeft, "[PAWN] from [Faction] withdrew after doing enough damage to the colony.");
    }

    private static void MockTriggerTime(Lord lord)
    {
        var (transition, trigger) = FindExitMapTrigger<Trigger_TicksPassed>(lord);
        Accessor.Trigger_TicksPassed.Duration(trigger) = 0;
        ((TriggerData_TicksPassed)trigger.data).ticksPassed = 1;
        transition.CheckSignal(lord, TriggerSignal.ForTick);
    }

    private static void MockDamageTaken(Lord lord)
    {
        var (transition, trigger) = FindExitMapTrigger<Trigger_FractionColonyDamageTaken>(lord);
        Accessor.Trigger_FractionColonyDamageTaken.DesiredColonyDamageFraction(trigger) = 0f;
        Accessor.Trigger_FractionColonyDamageTaken.MinDamage(trigger) = 1f;
        ((TriggerData_FractionColonyDamageTaken)trigger.data).startColonyDamage = -1f;
        transition.CheckSignal(lord, TriggerSignal.ForTick);
    }
}
