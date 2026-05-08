using System.Collections.Generic;
using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record DivorceByIdeoEvent(Pawn DivorcingPawn, List<Pawn> FormerSpouses) : GameEventBase;

internal record DivorceByIdeoState(List<Pawn> SpousesBefore);

[HarmonyPatch(typeof(SpouseRelationUtility), nameof(SpouseRelationUtility.RemoveSpousesAsForbiddenByIdeo))]
internal static class SpouseRelationUtility_RemoveSpousesAsForbiddenByIdeo_DivorceByIdeo_Patch
{
    private static void Prefix(Pawn pawn, out DivorceByIdeoState __state)
    {
        var spouses = pawn.GetCurrentSpouses();

        __state = new DivorceByIdeoState(spouses);
    }
    private static void Postfix(Pawn pawn, DivorceByIdeoState __state)
    {
        var formerSpouses = __state.SpousesBefore.ExceptList(pawn.GetCurrentSpouses());
        if (formerSpouses.Count == 0)
            return;
        
        GameEventBus.Publish(new DivorceByIdeoEvent(pawn, formerSpouses));
    }
}
