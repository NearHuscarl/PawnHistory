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

            if (lord.LordJob is not LordJob_TradeWithColony)
                return;

            if (isStartingLord)
                HandleCaravanTradeArrivedEvents(lord, pawns);
            if (!isStartingLord)
                HandleCaravanTradeLeftEvents(nextToil, lord, pawns, trigger, e.Signal);
        });
    }

    private void HandleCaravanTradeArrivedEvents(Lord lord, List<Pawn> pawns)
    {
        var recordDef = HistoryRecordDefOf.TradeCaravanArrived;
        var trader = pawns.FirstOrDefault(p => p.trader != null);
        var traderKind = trader?.trader?.traderKind?.label ?? "caravan";

        foreach (var pawn in pawns)
        {
            var desc = recordDef.Description(pawn)
                .AddRule("Faction", lord.faction)
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
        PackAnimalLost,
        PawnLost,
    }

    private void HandleCaravanTradeLeftEvents(LordToil nextToil, Lord lord, List<Pawn> pawns, Trigger trigger, TriggerSignal? signal)
    {
        if (nextToil is not LordToil_ExitMapAndEscortCarriers && nextToil is not LordToil_ExitMap && nextToil is not LordToil_ExitMapTraderFighting)
            return;

        var reason = CaravanLeftReason.Timeout;
        var trader = pawns.FirstOrDefault(p => p.trader != null);
        Pawn packAnimal = null;

        if (trigger is Trigger_PawnExperiencingDangerousTemperatures)
            reason = CaravanLeftReason.DangerousTemperature;
        else if (trigger is Trigger_PawnExperiencingAnomalousWeather)
            reason = CaravanLeftReason.AnomalousWeather;
        else if (trigger is Trigger_PawnCannotReachMapEdge)
            reason = CaravanLeftReason.Trapped;
        else if (trigger is Trigger_ImportantTraderCaravanPeopleLost)
        {
            if (signal?.Pawn.GetTraderCaravanRole() == TraderCaravanRole.Trader)
            {
                reason = CaravanLeftReason.TraderLost;
                trader = signal?.Pawn;
            }
            else if (signal?.Pawn.RaceProps.packAnimal ?? false)
            {
                reason = CaravanLeftReason.PackAnimalLost;
                packAnimal = signal?.Pawn;
            }
        }
        else if (trigger is Trigger_PawnLost || trigger is Trigger_FractionPawnsLost)
            reason = CaravanLeftReason.PawnLost;

        var traderKind = trader?.trader?.traderKind?.label ?? "trader";
        var recordDef = HistoryRecordDefOf.TradeCaravanLeft;

        foreach (var pawn in pawns)
        {
            var desc = recordDef.Description(pawn)
                .IncludePawnGrammar()
                .WithOthers(pawns)
                .AddRule("Faction", lord.faction)
                .AddRule("Trader", trader)
                .AddRule("PackAnimal", packAnimal, addSubsymbols: true)
                .AddRule("TraderKind", traderKind)
                .AddConstant("reason", reason)
                .Resolve();
            AddRecord(recordDef, pawn, desc, [signal?.Pawn]);
        }
    }

    public void TestFractionLost(TestScenario scenario)
    {
        var pawns = scenario.Incident(IncidentDefOf.TraderCaravanArrival).Point(400).Execute();

        TickDelayManager.Delay(200, () =>
        {
            pawns.Where(p => !p.RaceProps.packAnimal && p.trader == null)
                .ToList()
                .ForEach(p => HealthUtility.DamageUntilDead(p));
        });
    }

    public void TestAnimalLost(TestScenario scenario)
    {
        var pawns = scenario.Incident(IncidentDefOf.TraderCaravanArrival).Point(400).Execute();

        TickDelayManager.Delay(200, () =>
        {
            var animal = pawns.FirstOrDefault(p => p.RaceProps.packAnimal);
            if (animal != null)
                HealthUtility.DamageUntilDead(animal);
        });
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
