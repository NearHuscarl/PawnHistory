using System.Collections.Generic;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PawnTradedRecorder : RecorderBase<PawnTradedRecorder.Input>
{
    public record Input(Pawn SoldVictim, Pawn Negotiator, ITrader Trader, int Price, TradeAction TradeAction);
    
    public override void Register()
    {
        GameEventBus.Subscribe<TradedEvent>(e =>
        {
            foreach (var tradeable in e.Tradeables)
            {
                if (tradeable.AnyThing is not Pawn soldVictim)
                    continue;

                var price = tradeable.CostToInt(tradeable.GetPriceFor(tradeable.ActionToDo));
                var tradeAction = tradeable.ActionToDo;
                CreateRecord(new Input(soldVictim, e.Negotiator, e.Trader, price, tradeAction));
            }
        });
    }

    public override void CreateRecord(Input input)
    {
        var (soldVictim, negotiator, trader, price, tradeAction) = input;
        
        if (!ShouldRecord(soldVictim))
            return;

        var recordDef = tradeAction == TradeAction.PlayerSells ? HistoryRecordDefOf.SoldToSlavery : HistoryRecordDefOf.BoughtFromSlavery;
        var desc = recordDef.Description(soldVictim)
            .AddRule("Negotiator", negotiator)
            .AddRule("Trader", TraderUtility.GetName(trader))
            .AddRule("Price", price)
            .AddConstant("action", tradeAction)
            .Resolve();
        AddRecord(recordDef, soldVictim, desc, [negotiator]);
    }

    public void TestSell(TestScenario scenario)
    {
        var traderKind = DefLookup.TraderKind.Caravan_Neolithic_Slaver;
        scenario.Incident(IncidentDefOf.TraderCaravanArrival).TraderKind(traderKind).Execute();
        var prisoners = new List<Pawn>();

        scenario.Map()
            .BuildRoom(8, 8, tag: "Prison")
            .AsPrison(2, prisoners: prisoners)
            .WithThing(ThingDefOf.Silver, 3000)
            .Execute();

        var negotiator = scenario.Pawn().Colonist().CreateSingle();
        var result = scenario.Trade(negotiator)
            .WithCaravanTrader(traderKind)
            .Sell(t => t.AnyThing is Pawn)
            .Execute();
        
        Expect.That(result.Sold[0] as Pawn).ToHaveHistoryRecord("[Negotiator] sold [PAWN] into slavery to [Trader] for [x] silvers.");
    }

    public void TestBuy(TestScenario scenario)
    {
        var traderKind = DefLookup.TraderKind.Caravan_Neolithic_Slaver;
        scenario.Incident(IncidentDefOf.TraderCaravanArrival).TraderKind(traderKind).Execute();

        scenario.Map()
            .BuildRoom(8, 8, tag: "Prison")
            .AsPrison(2)
            .WithThing(ThingDefOf.Silver, 3000)
            .Execute();

        var negotiator = scenario.Pawn().Colonist().CreateSingle();
        var result = scenario.Trade(negotiator)
            .WithCaravanTrader(traderKind)
            .Buy(t => t.AnyThing is Pawn)
            .Execute();
        
        Expect.That(result.Bought[0] as Pawn).ToHaveHistoryRecord("[Negotiator] bought [PAWN] from [Trader] for [x] silvers.");
    }

    public void TestBuy2(TestScenario scenario)
    {
        var traderKind = DefLookup.TraderKind.Orbital_PirateMerchant;
        scenario.Incident(IncidentDefOf.OrbitalTraderArrival).TraderKind(traderKind).Execute();

        scenario.Map()
            .BuildRoom(6, 6, tag: "Prison")
            .AsPrison(2)
            .Execute();
        
        scenario.Map()
            .BuildRoom(MapBuilder.Beside("Prison", Rot4.East, 6, 6), tag: "Comm")
            .WithThing(ThingDefOf.CommsConsole, 1, Faction.OfPlayer)
            .WithThing(ThingDefOf.OrbitalTradeBeacon, 1, Faction.OfPlayer)
            .WithThing(DefLookup.Thing.VanometricPowerCell, 1, Faction.OfPlayer)
            .WithThing(ThingDefOf.Silver, 3000)
            .Execute();

        var negotiator = scenario.Pawn().Colonist().CreateSingle();
        var result = scenario.Trade(negotiator)
            .WithOrbitalTrader(traderKind)
            .Buy(t => t.AnyThing is Pawn)
            .Execute();
        
        Expect.That(result.Bought[0] as Pawn).ToHaveHistoryRecord("[Negotiator] bought [PAWN] from [Trader] for [x] silvers.");
    }
}
