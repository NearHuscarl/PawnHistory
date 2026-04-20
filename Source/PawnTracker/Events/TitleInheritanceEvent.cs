using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum TitleInheritanceOutcome
{
    AsReplacement,
    WasInherited,
}

public record TitleInheritanceEvent(Pawn Heir, Pawn Deceased, Faction Faction, RoyalTitleDef Title, TitleInheritanceOutcome Outcome, RoyalTitleDef HeirCurrentTitle) : GameEventBase;

internal static class TitleInheritanceContext
{
    public static bool IsActive;
}

[HarmonyPatch(typeof(Pawn_RoyaltyTracker), nameof(Pawn_RoyaltyTracker.Notify_PawnKilled))]
internal static class Pawn_RoyaltyTracker_Notify_PawnKilled_Patch
{
    private static void Prefix() => TitleInheritanceContext.IsActive = true;
    private static void Finalizer() => TitleInheritanceContext.IsActive = false;
}

[HarmonyPatch(
    typeof(RoyalTitleDefExt),
    nameof(RoyalTitleDefExt.TryInherit),
    [typeof(RoyalTitleDef), typeof(Pawn), typeof(Faction), typeof(RoyalTitleInheritanceOutcome)],
    [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out]
)]
internal static class RoyalTitleDefExt_TryInherit_Patch
{
    private static void Postfix(bool __result, RoyalTitleDef title, Pawn from, Faction faction, RoyalTitleInheritanceOutcome outcome)
    {
        if (!TitleInheritanceContext.IsActive || !__result)
            return;

        var inheritanceOutcome = outcome.HeirHasTitle ? TitleInheritanceOutcome.AsReplacement : TitleInheritanceOutcome.WasInherited;

        GameEventBus.Publish(new TitleInheritanceEvent(
            outcome.heir,
            from,
            faction,
            title,
            inheritanceOutcome,
            outcome.heirCurrentTitle
        ));
    }
}