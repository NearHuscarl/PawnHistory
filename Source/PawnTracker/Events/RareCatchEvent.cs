using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record RareCatchEvent(Pawn Pawn, List<Thing> Catches) : GameEventBase;

[HarmonyPatch(typeof(FishingUtility), nameof(FishingUtility.GetCatchesFor))]
internal static class FishingUtility_GetCatchesFor_Patch
{
    private static void Postfix(Pawn pawn, ref bool rare, List<Thing> __result)
    {
        if (!rare)
            return;

        GameEventBus.Publish(new RareCatchEvent(pawn, __result.ToList()));
    }
}
