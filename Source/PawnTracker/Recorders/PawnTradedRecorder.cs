using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PawnTradedRecorder : RecorderBase<PawnTradedRecorder.Input>
{
    public record Input(Pawn SoldVictim, Pawn Negotiator, Pawn Trader, int Price, TradeAction TradeAction);
    
    public override void Register()
    {
        GameEventBus.Subscribe<TradedEvent>(e =>
        {
            // TODO: not necessary but needs more tests
            if (e.Trader is not Pawn trader)
                return;
            
            foreach (var tradeable in e.Tradeables)
            {
                if (tradeable.AnyThing is not Pawn soldVictim)
                    continue;

                var price = tradeable.CostToInt(tradeable.GetPriceFor(tradeable.ActionToDo));
                var tradeAction = tradeable.ActionToDo;
                CreateRecord(new Input(soldVictim, e.Negotiator, trader, price, tradeAction));
            }
        });
    }

    public override void CreateRecord(Input input)
    {
        if (!ShouldRecord(input.SoldVictim))
            return;

        var recordDef = input.TradeAction == TradeAction.PlayerSells ? HistoryRecordDefOf.SoldToSlavery : HistoryRecordDefOf.BoughtFromSlavery;
        var desc = recordDef.Description(input.SoldVictim)
            .AddRule("Negotiator", input.Negotiator)
            .AddRule("Faction", input.Trader.Faction)
            .AddRule("Price", input.Price)
            .AddConstant("action", input.TradeAction)
            .Resolve();
        AddRecord(recordDef, input.SoldVictim, desc, [input.Negotiator]);
    }

    public void TestSell(TestScenario scenario)
    {
        var pawns = scenario.Incident(IncidentDefOf.TraderCaravanArrival)
            .TraderKind(DefLookup.TraderKind.Caravan_Neolithic_Slaver)
            .Point(400)
            .Execute();
        var trader = pawns[0];
        var prisoners = new List<Pawn>();

        scenario.Map()
            .BuildRoom(8, 8, tag: "Prison")
            .AsPrison(2, prisoners: prisoners)
            .WithThing(ThingDefOf.Silver, 3000)
            .Execute();

        var negotiator = scenario.Pawn().Colonist().CreateSingle();
        var result = scenario.Trade(trader, negotiator)
            .Sell(t => t is Pawn && t.Faction != trader.Faction)
            .Execute();
        
        Expect.That(result.Sold[0] as Pawn).ToHaveHistoryRecord("[Negotiator] sold [PAWN] into slavery to [Faction] for [x] silvers.");
    }

    public void TestBuy(TestScenario scenario)
    {
        var pawns = scenario.Incident(IncidentDefOf.TraderCaravanArrival)
            .TraderKind(DefLookup.TraderKind.Caravan_Neolithic_Slaver)
            .Point(400)
            .Execute();
        var trader = pawns[0];

        scenario.Map()
            .BuildRoom(8, 8, tag: "Prison")
            .AsPrison(2)
            .WithThing(ThingDefOf.Silver, 3000)
            .Execute();

        var negotiator = scenario.Pawn().Colonist().CreateSingle();
        var result = scenario.Trade(trader, negotiator)
            .Buy(t => t is Pawn && t.Faction == trader.Faction)
            .Execute();
        
        Expect.That(result.Bought[0] as Pawn).ToHaveHistoryRecord("[Negotiator] bought [PAWN] from [Faction] for [x] silvers.");
    }
}
