using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum BanishReason
{
    None,
    CubeDestroyed,
}

public record BanishEvent(Pawn Pawn, bool LeftToDie, BanishReason Reason) : GameEventBase;

internal static class BanishContext
{
    public static BanishReason Reason;
}

[HarmonyPatch(typeof(PawnBanishUtility), nameof(PawnBanishUtility.Banish), typeof(Pawn), typeof(PlanetTile), typeof(bool))]
internal static class PawnBanishUtility_Banish_Patch
{
    // fires in prefix as pawn can be killed during banish call. This maintains the order with the Death record.
    private static void Prefix(Pawn pawn, PlanetTile tile)
    {
        var leftToDie = PawnBanishUtility.WouldBeLeftToDie(pawn, tile);
        GameEventBus.Publish(new BanishEvent(pawn, leftToDie, BanishContext.Reason));
        BanishContext.Reason = BanishReason.None;
    }
}

[HarmonyPatch(typeof(CompGoldenCube), "OnInteracted")]
internal static class CompGoldenCube_OnInteracted_Patch
{
    private static void Prefix() => BanishContext.Reason = BanishReason.CubeDestroyed;
}
