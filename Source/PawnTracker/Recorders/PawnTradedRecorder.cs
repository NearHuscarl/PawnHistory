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
            foreach (var tradedItem in e.TradedItems)
            {
                if (tradedItem.Thing is not Pawn soldVictim)
                    continue;

                CreateRecord(new Input(soldVictim, e.Negotiator, e.Trader, tradedItem.Price, tradedItem.Action));
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
        var traderKind = Extra.TraderKindDefOf.Caravan_Neolithic_Slaver;
        scenario.Incident(IncidentDefOf.TraderCaravanArrival).TraderKind(traderKind).Execute();
        var prisoners = new List<Pawn>();

        scenario.Map()
            .BuildRoom(8, 8, tag: "Prison")
            .AsPrison(2, prisoners: prisoners)
            .Execute();

        var negotiator = scenario.Pawn().Colonist().CreateSingle();
        var result = scenario.Trade(negotiator)
            .WithCaravanTrader(traderKind)
            .Sell(t => t.AnyThing is Pawn);
        
        Expect.That(result.Sold[0] as Pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.SoldToSlavery,
            Description = "[Negotiator] sold [PAWN] into slavery to [Trader] for [x] silvers.",
            Concerns = [negotiator],
        });
    }

    [TestTag("Flaky")]
    public void TestBuy(TestScenario scenario)
    {
        var traderKind = Extra.TraderKindDefOf.Caravan_Neolithic_Slaver;
        scenario.Incident(IncidentDefOf.TraderCaravanArrival).TraderKind(traderKind).Execute();

        scenario.Map()
            .BuildRoom(8, 8, tag: "Prison")
            .AsPrison(2)
            .WithThing(ThingDefOf.Silver, 3000)
            .Execute();

        var negotiator = scenario.Pawn().Colonist().CreateSingle();
        var result = scenario.Trade(negotiator)
            .WithCaravanTrader(traderKind)
            .Buy(t => t.AnyThing is Pawn);
        
        Expect.That(result.Bought[0] as Pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BoughtFromSlavery,
            Description = "[Negotiator] bought [PAWN] from [Trader] for [x] silvers.",
            Concerns = [negotiator],
        });
    }

    public void TestBuy2(TestScenario scenario)
    {
        var traderKind = Extra.TraderKindDefOf.Orbital_PirateMerchant;
        scenario.Incident(IncidentDefOf.OrbitalTraderArrival).TraderKind(traderKind).Execute();

        scenario.Map()
            .BuildRoom(6, 6, tag: "Prison")
            .AsPrison(2)
            .Execute();
        
        scenario.Map()
            .BuildRoom(MapBuilder.Beside("Prison", Rot4.East, 6, 6), tag: "Comm")
            .AsBank()
            .Execute();

        var negotiator = scenario.Pawn().Colonist().CreateSingle();
        var result = scenario.Trade(negotiator)
            .WithOrbitalTrader(traderKind)
            .Buy(t => t.AnyThing is Pawn);
        
        Expect.That(result.Bought[0] as Pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BoughtFromSlavery,
            Description = "[Negotiator] bought [PAWN] from [Trader] for [x] silvers.",
            Concerns = [negotiator],
        });
    }
}
