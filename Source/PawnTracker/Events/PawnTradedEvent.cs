using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record PawnTradedEvent(Pawn SoldVictim, Pawn Negotiator, Pawn Trader, int Price, TradeAction TradeAction) : GameEventBase;

class PawnTradedContext
{
    public static Dictionary<Pawn, List<Tradeable>> PendingTradables = [];
}

[HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.TryExecute))]
internal static class TradeDeal_TryExecute_Patch
{
    static void Prefix(TradeDeal __instance)
    {
        var negotiator = TradeSession.playerNegotiator;

        foreach (var tradeable in __instance.AllTradeables)
        {
            if (tradeable.AnyThing is not Pawn)
                continue;

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

    static void Postfix(bool __result)
    {
        var negotiator = TradeSession.playerNegotiator;
        var trader = TradeSession.trader as Pawn;

        if (!__result)
        {
            PawnTradedContext.PendingTradables.Remove(negotiator);
            return;
        }

        PawnTradedContext.PendingTradables.TryGetValue(negotiator, out List<Tradeable> tradeables);
        foreach (var tradeable in tradeables)
        {
            if (tradeable.AnyThing is not Pawn soldVictim)
                continue;

            var price = tradeable.GetPriceFor(tradeable.ActionToDo);
            var tradeAction = tradeable.ActionToDo;
            GameEventBus.Publish(new PawnTradedEvent(soldVictim, negotiator, trader, tradeable.CostToInt(price), tradeAction));
        }

        PawnTradedContext.PendingTradables.Remove(negotiator);
    }
}