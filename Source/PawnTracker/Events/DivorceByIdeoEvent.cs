using System.Collections.Generic;
using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record DivorceByIdeoEvent(Pawn DivorcingPawn, List<Pawn> FormerSpouses) : GameEventBase;

internal record DivorceByIdeoState(List<Pawn> SpousesBefore);

[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.SetIdeo))]
internal static class Pawn_IdeoTracker_SetIdeo_Patch_3
{
    private static void Prefix(Pawn_IdeoTracker __instance, out DivorceByIdeoState __state)
    {
        var pawn = Accessor.Pawn_IdeoTracker.Pawn(__instance);
        __state = new DivorceByIdeoState(pawn.GetPawnsWithRelation(PawnRelationDefOf.Spouse));
    }

    private static void Postfix(Pawn_IdeoTracker __instance, DivorceByIdeoState __state)
    {
        var pawn = Accessor.Pawn_IdeoTracker.Pawn(__instance);
        var formerSpouses = __state.SpousesBefore.ExceptList(pawn.GetPawnsWithRelation(PawnRelationDefOf.Spouse));
        if (formerSpouses.Count == 0)
            return;

        GameEventBus.Publish(new DivorceByIdeoEvent(pawn, formerSpouses));
    }
}
