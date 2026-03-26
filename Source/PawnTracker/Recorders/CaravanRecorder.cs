using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class CaravanRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<LordToilChangeEvent>(e =>
        {
            var lord = e.Lord;
            var currentToil = e.CurrentToil;
            var nextToil = e.NextToil;
            var trigger = e.Trigger;
            var pawns = lord.ownedPawns.Where(ShouldRecord).ToList();
            var isStartingLord = currentToil == null;

            //Log.Message($"{lord.LordJob}: {currentToil}->{nextToil} trigger={trigger?.GetType().Name}");
            if (lord.LordJob is not LordJob_TradeWithColony)
                return;

            if (isStartingLord)
                HandleCaravanTradeArrivedEvents(lord, pawns);
            if (!isStartingLord)
                HandleCaravanTradeLeftEvents(nextToil, lord, pawns, trigger);
        });
    }

    private void HandleCaravanTradeArrivedEvents(Lord lord, List<Pawn> pawns)
    {
        var recordDef = HistoryRecordDefOf.TradeCaravanArrived;
        var trader = pawns.FirstOrDefault(p => p.trader != null);
        var traderKind = trader?.trader?.traderKind?.label ?? "trader";

        foreach (var pawn in pawns)
        {
            var desc = recordDef.Description(pawn)
                .WithFaction(lord.faction)
                .AddRule("TraderKind", traderKind)
                .Resolve();
            AddRecord(recordDef, pawn, desc);
        }
    }

    enum CaravanLeftReason
    {
        Timeout,
        DangerousTemperature,
        AnomalousWeather,
        Trapped,
        TraderLost,
        PawnLost,
    }

    private void HandleCaravanTradeLeftEvents(LordToil nextToil, Lord lord, List<Pawn> pawns, Trigger trigger)
    {
        var trader = pawns.FirstOrDefault(p => p.trader != null);
        var traderKind = trader?.trader?.traderKind?.label ?? "trader";
        var reason = CaravanLeftReason.Timeout;

        if (trigger is Trigger_PawnExperiencingDangerousTemperatures)
            reason = CaravanLeftReason.DangerousTemperature;
        else if (trigger is Trigger_PawnExperiencingAnomalousWeather)
            reason = CaravanLeftReason.AnomalousWeather;
        else if (trigger is Trigger_PawnCannotReachMapEdge)
            reason = CaravanLeftReason.Trapped;
        else if (trigger is Trigger_ImportantTraderCaravanPeopleLost)
            reason = CaravanLeftReason.TraderLost;
        else if (trigger is Trigger_PawnLost || trigger is Trigger_FractionPawnsLost)
            reason = CaravanLeftReason.PawnLost;

        if (nextToil is LordToil_ExitMapAndEscortCarriers
            || nextToil is LordToil_ExitMap
            || nextToil is LordToil_ExitMapTraderFighting)
        {
            var recordDef = HistoryRecordDefOf.TradeCaravanLeft;

            foreach (var pawn in pawns)
            {     
                var desc = recordDef.Description(pawn)
                    .WithFaction(lord.faction)
                    .WithOthers(pawns)
                    .AddRule("TraderKind", traderKind)
                    .AddConstant("reason", reason)
                    .Resolve();
                AddRecord(recordDef, pawn, desc);
            }
        }
    }

    public override void Test(TestScenario scenario)
    {
        scenario.Incident(IncidentDefOf.TraderCaravanArrival).Point(400).Execute();
    }

    public void TestTraderLost(TestScenario scenario)
    {
        var pawns = scenario.Incident(IncidentDefOf.TraderCaravanArrival).Point(400).Execute();

        TickDelayManager.Delay(200, () =>
        {
            var trader = pawns.FirstOrDefault(p => p.trader != null);
            if (trader != null)
            {
                HealthUtility.DamageUntilDead(trader);
                scenario.OpenHistoryRecordTab(trader);
            }
        });
    }
}
