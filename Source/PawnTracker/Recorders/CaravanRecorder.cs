using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Recorders;

public record CaravanArrivedInput(Faction Faction, List<Pawn> Pawns);

public record CaravanLeftInput(LordToil NextToil, Lord Lord, List<Pawn> Pawns, Trigger Trigger, TriggerSignal? Signal);

public class CaravanRecorder : RecorderBase<CaravanArrivedInput>, IRecord<CaravanLeftInput>
{
    public override void Register()
    {
        GameEventBus.Subscribe<LordToilChangeEvent>(e =>
        {
            var (currentToil, nextToil, trigger, lord, signal) = e;
            var pawns = lord.ownedPawns;
            var isStartingLord = currentToil == null;

            if (lord.LordJob is not LordJob_TradeWithColony)
                return;

            if (isStartingLord)
                CreateRecord(new CaravanArrivedInput(lord.faction, pawns));
            if (!isStartingLord)
                CreateRecord(new CaravanLeftInput(nextToil, lord, pawns, trigger, signal));
        });
    }

    public override void CreateRecord(CaravanArrivedInput input)
    {
        var (faction, pawns) = input;
        var recordDef = HistoryRecordDefOf.TradeCaravanArrived;
        var trader = pawns.FirstOrDefault(p => p.trader != null);
        var traderKind = trader?.trader?.traderKind?.label ?? "caravan";

        foreach (var pawn in pawns)
        {
            if (!ShouldRecord(pawn)) continue;

            var desc = recordDef.Description(pawn)
                .AddRule("Faction", faction)
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

    public void CreateRecord(CaravanLeftInput input)
    {
        var (nextToil, lord, pawns, trigger, signal) = input;
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
        else if (trigger is Trigger_PawnLost or Trigger_FractionPawnsLost)
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

    [SkipTest]
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

    [SkipTest]
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

    [SkipTest]
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
