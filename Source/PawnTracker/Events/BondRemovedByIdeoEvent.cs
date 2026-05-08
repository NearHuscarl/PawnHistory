using System.Collections.Generic;
using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record BondRemovedByIdeoEvent(Pawn Pawn, List<Pawn> FormerBondedAnimals) : GameEventBase;

internal record BondRemovedByIdeoState(List<Pawn> BondedAnimalsBefore);

[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.SetIdeo))]
internal static class Pawn_IdeoTracker_SetIdeo_Patch_2
{
    private static void Prefix(Pawn_IdeoTracker __instance, out BondRemovedByIdeoState __state)
    {
        var pawn = Accessor.Pawn_IdeoTracker.Pawn(__instance);
        __state = new BondRemovedByIdeoState(pawn.GetPawnsWithRelation(PawnRelationDefOf.Bond));
    }

    private static void Postfix(Pawn_IdeoTracker __instance, BondRemovedByIdeoState __state)
    {
        var pawn = Accessor.Pawn_IdeoTracker.Pawn(__instance);
        var formerBondedAnimals = __state.BondedAnimalsBefore.ExceptList(pawn.GetPawnsWithRelation(PawnRelationDefOf.Bond));
        if (formerBondedAnimals.Count == 0)
            return;

        GameEventBus.Publish(new BondRemovedByIdeoEvent(pawn, formerBondedAnimals));
    }
}
