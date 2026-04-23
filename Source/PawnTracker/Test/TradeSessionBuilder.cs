using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class TradeSessionBuilder
{
    private readonly List<Action> processors = [];
    
    private readonly ITrader trader;
    private readonly Pawn negotiator;
    private bool giftMode;
    private readonly TradeSessionResult result = new([], [], false);
    
    public record TradeSessionResult(List<Thing> Bought, List<Thing> Sold, bool ActuallyTraded);

    public TradeSessionBuilder(ITrader trader, Pawn negotiator)
    {
        this.trader = trader;
        this.negotiator = negotiator;
    }

    public TradeSessionBuilder Buy(Func<Tradeable, bool> filter = null, int count = -1)
    {
        processors.Add(() =>
        {
            var tradeable = TradeSession.deal.AllTradeables.Where(t => t.CountHeldBy(Transactor.Trader) > 0).FirstOrDefault(t => filter?.Invoke(t) ?? true);
            if (tradeable == null)
                return;
            
            var adjustment = count == -1 ? tradeable.AnyThing.stackCount : count;
            tradeable.AdjustBy(adjustment);
            result.Sold.Add(tradeable.AnyThing);
        });
        return this;
    }

    public TradeSessionBuilder Sell(Func<Tradeable, bool> filter = null, int count = -1)
    {
        processors.Add(() =>
        {
            var tradeable = TradeSession.deal.AllTradeables.Where(t => t.CountHeldBy(Transactor.Colony) > 0).FirstOrDefault(t => filter?.Invoke(t) ?? true);
            if (tradeable == null)
                return;
            
            var adjustment = count == -1 ? tradeable.AnyThing.stackCount : count;
            tradeable.AdjustBy(-adjustment);
            result.Sold.Add(tradeable.AnyThing);
        });
        return this;
    }

    public TradeSessionBuilder Gift(Func<Tradeable, bool> filter = null, int count = -1)
    {
        giftMode = true;
        return Sell(filter, count);
    }

    public TradeSessionResult Execute()
    {
        TradeSession.SetupWith(trader, negotiator, giftMode);

        processors.ForEach(processor => processor());
        
        TradeSession.deal.TryExecute(out var actuallyTraded);
        
        return result with { ActuallyTraded = actuallyTraded };
    }
}