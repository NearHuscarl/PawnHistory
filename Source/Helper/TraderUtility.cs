using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.Helper;

public static class TraderUtility
{
    public static string GetName(ITrader trader)
    {
        if (trader is Pawn)
            return trader.Faction.NameColored.Resolve();
        if (trader is TradeShip)
            return trader.Faction?.NameColored.Resolve() ?? trader.TraderName;
        if (trader is Settlement)
            return trader.Faction.NameColored.Resolve();

        return trader.ToString();
    }
}