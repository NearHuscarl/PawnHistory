using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class TradeDealBuilder
{
    private readonly List<Action> processors = [];
    
    private ITrader trader;
    private readonly Pawn negotiator;
    private bool giftMode;
    private Map Map => negotiator.MapHeld;
    private readonly TradeSessionResult result = new([], [], false);
    
    public record TradeSessionResult(List<Thing> Bought, List<Thing> Sold, bool ActuallyTraded);

    public TradeDealBuilder(Pawn negotiator)
    {
        this.negotiator = negotiator;
    }

    public TradeDealBuilder WithCaravanTrader(TraderKindDef traderKindDef)
    {
        if (traderKindDef.orbital)
        {
            Log.Warning($"Expect a caravan trader kind but got an orbital trader kind: {traderKindDef.defName}");
            return this;
        }

        trader = Map.mapPawns.AllHumanlikeSpawned.FirstOrDefault(p => p.trader?.traderKind == traderKindDef);
        return this;
    }

    public TradeDealBuilder WithOrbitalTrader(TraderKindDef traderKindDef)
    {
        if (!traderKindDef.orbital)
        {
            Log.Warning($"Expect an orbital trader kind but got an caravan trader kind: {traderKindDef.defName}");
            return this;
        }

        trader = Map.passingShipManager.passingShips.OfType<TradeShip>().FirstOrDefault(s => s.TraderKind == traderKindDef);
        return this;
    }
    
    public TradeDealBuilder WithSettlementTrader(Settlement settlement)
    {
        trader = settlement;
        return this;
    }

    public TradeSessionResult Buy(Func<Tradeable, bool> filter = null, int count = -1)
    {
        processors.Add(() =>
        {
            var tradeable = TradeSession.deal.AllTradeables.Where(t => t.CountHeldBy(Transactor.Trader) > 0).FirstOrDefault(t => filter?.Invoke(t) ?? true);
            if (tradeable == null)
                return;
            
            var heldByTrader = tradeable.CountHeldBy(Transactor.Trader);
            var adjustment = count == -1 ? heldByTrader : Math.Min(count, heldByTrader);
            tradeable.AdjustBy(adjustment);
            result.Bought.Add(tradeable.AnyThing);
        });
        return Execute();
    }

    public TradeSessionResult Sell(Func<Tradeable, bool> filter = null, int count = -1)
    {
        processors.Add(() =>
        {
            var tradeable = TradeSession.deal.AllTradeables.Where(t => t.CountHeldBy(Transactor.Colony) > 0).FirstOrDefault(t => filter?.Invoke(t) ?? true);
            if (tradeable == null)
                return;
            
            var heldByColony = tradeable.CountHeldBy(Transactor.Colony);
            var adjustment = count == -1 ? heldByColony : Math.Min(count, heldByColony);
            tradeable.AdjustBy(-adjustment);
            result.Sold.Add(tradeable.AnyThing);
        });
        return Execute();
    }

    public TradeSessionResult Gift(Func<Tradeable, bool> filter = null, int count = -1)
    {
        giftMode = true;
        processors.Add(() =>
        {
            var tradeable = TradeSession.deal.AllTradeables.Where(t => t.CountHeldBy(Transactor.Colony) > 0).FirstOrDefault(t => filter?.Invoke(t) ?? true);
            if (tradeable == null)
                return;
            
            var heldByColony = tradeable.CountHeldBy(Transactor.Colony);
            var adjustment = count == -1 ? heldByColony : Math.Min(count, heldByColony);
            tradeable.AdjustBy(adjustment);
            result.Sold.Add(tradeable.AnyThing);
        });
        return Execute();
    }

    private TradeSessionResult Execute()
    {
        if (trader == null)
            throw new InvalidOperationException("No trader was selected or found.");
        
        TradeSession.SetupWith(trader, negotiator, giftMode);

        processors.ForEach(processor => processor());
        
        TradeSession.deal.TryExecute(out var actuallyTraded);
        
        return result with { ActuallyTraded = actuallyTraded };
    }
}