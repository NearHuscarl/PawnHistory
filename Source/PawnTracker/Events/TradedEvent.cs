using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record TradedEvent(Pawn Negotiator, ITrader Trader, List<Tradeable> Tradeables, bool GiftMode = false) : GameEventBase;

internal static class PawnTradedContext
{
    public static readonly Dictionary<Pawn, List<Tradeable>> PendingTradables = [];
}

[HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.TryExecute))]
internal static class TradeDeal_TryExecute_Patch
{
    private static void Prefix(TradeDeal __instance)
    {
        var negotiator = TradeSession.playerNegotiator;

        foreach (var tradeable in __instance.AllTradeables)
        {
            if (tradeable.ActionToDo == TradeAction.None)
                continue;

            if (!PawnTradedContext.PendingTradables.TryGetValue(negotiator, out var tradeables))
            {
                tradeables = [];
                PawnTradedContext.PendingTradables[negotiator] = tradeables;
            }

            tradeables.Add(tradeable);
        }
    }

    private static void Postfix(bool __result)
    {
        var negotiator = TradeSession.playerNegotiator;
        var trader = TradeSession.trader;

        if (!__result)
        {
            PawnTradedContext.PendingTradables.Remove(negotiator);
            return;
        }

        PawnTradedContext.PendingTradables.TryGetValue(negotiator, out var tradeables);
        GameEventBus.Publish(new TradedEvent(negotiator, trader, tradeables, TradeSession.giftMode));

        PawnTradedContext.PendingTradables.Remove(negotiator);
    }
}