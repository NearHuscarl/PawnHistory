using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record TradedEvent(Pawn Negotiator, ITrader Trader, List<TradedItem> TradedItems, bool GiftMode = false) : GameEventBase;

public record TradedItem(Thing Thing, int Price, TradeAction Action);

internal class PawnTradedState
{
    public readonly List<TradedItem> PendingItems = [];
}

[HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.TryExecute))]
internal static class TradeDeal_TryExecute_Patch
{
    private static void Prefix(TradeDeal __instance, out PawnTradedState __state)
    {
        __state = new PawnTradedState();

        foreach (var tradeable in __instance.AllTradeables)
        {
            if (tradeable.ActionToDo == TradeAction.None)
                continue;
            
            __state.PendingItems.Add(new TradedItem(
                tradeable.AnyThing, // may not safe to get after trade deal completed
                tradeable.CostToInt(tradeable.GetPriceFor(tradeable.ActionToDo)),
                tradeable.ActionToDo
            ));
        }
    }

    private static void Postfix(bool __result, PawnTradedState __state, bool actuallyTraded)
    {
        var negotiator = TradeSession.playerNegotiator;
        var trader = TradeSession.trader;

        if (!__result || !actuallyTraded)
            return;

        GameEventBus.Publish(new TradedEvent(negotiator, trader, __state.PendingItems, TradeSession.giftMode));
    }
}