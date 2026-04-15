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

    public TradeSessionBuilder GifMode(bool value = true)
    {
        this.giftMode = value;
        return this;
    }

    public TradeSessionBuilder Buy(Func<Thing, bool> filter, int count = 1)
    {
        processors.Add(() =>
        {
            TradeSession.deal.AllTradeables.Where(t => filter(t.AnyThing)).Take(count).ToList().ForEach(t =>
            {
                t.AdjustBy(1);
                result.Bought.Add(t.AnyThing);
            });
        });
        return this;
    }

    public TradeSessionBuilder Sell(Func<Thing, bool> filter, int count = 1)
    {
        processors.Add(() =>
        {
            TradeSession.deal.AllTradeables.Where(t => filter(t.AnyThing)).Take(count).ToList().ForEach(t =>
            {
                t.AdjustBy(-1);
                result.Sold.Add(t.AnyThing);
            });
        });
        return this;
    }

    public TradeSessionResult Execute()
    {
        TradeSession.SetupWith(trader, negotiator, giftMode);

        processors.ForEach(processor => processor());
        
        TradeSession.deal.TryExecute(out var actuallyTraded);
        
        return result with { ActuallyTraded = actuallyTraded };
    }
}