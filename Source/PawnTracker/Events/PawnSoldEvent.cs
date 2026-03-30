using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public class PawnSoldEvent(Pawn soldVictim, Pawn negotiator, Pawn trader, int price, TradeAction tradeAction) : GameEventBase
{
    public Pawn SoldVictim { get; } = soldVictim;
    public Pawn Negotiator { get; } = negotiator;
    public Pawn Trader { get; } = trader;
    public int Price { get; } = price;
    public TradeAction TradeAction { get; } = tradeAction;
}

class PawnSoldContext
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

            if (!PawnSoldContext.PendingTradables.TryGetValue(negotiator, out var tradeables))
            {
                tradeables = [];
                PawnSoldContext.PendingTradables[negotiator] = tradeables;
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
            PawnSoldContext.PendingTradables.Remove(negotiator);
            return;
        }

        PawnSoldContext.PendingTradables.TryGetValue(negotiator, out List<Tradeable> tradeables);
        foreach (var tradeable in tradeables)
        {
            if (tradeable.AnyThing is not Pawn soldVictim)
                continue;

            var price = tradeable.GetPriceFor(tradeable.ActionToDo);
            var tradeAction = tradeable.ActionToDo;
            GameEventBus.Publish(new PawnSoldEvent(soldVictim, negotiator, trader, tradeable.CostToInt(price), tradeAction));
        }

        PawnSoldContext.PendingTradables.Remove(negotiator);
    }
}