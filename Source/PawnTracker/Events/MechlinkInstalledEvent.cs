using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record MechlinkInstalledEvent(Pawn Pawn) : GameEventBase;

[HarmonyPatch(typeof(Hediff_Mechlink), nameof(Hediff_Mechlink.PostAdd))]
internal static class Hediff_Mechlink_PostAdd_Patch
{
    private static void Postfix(Hediff_Mechlink __instance)
    {
        var pawn = __instance.pawn;
        if (!pawn.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant))
            return;

        GameEventBus.Publish(new MechlinkInstalledEvent(pawn));
    }
}
