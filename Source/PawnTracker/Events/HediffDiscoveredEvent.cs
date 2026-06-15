using HarmonyLib;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record HediffDiscoveredEvent(Pawn Pawn, Hediff Hediff, BodyPartRecord Part) : GameEventBase;

[HarmonyPatch(typeof(HediffComp_Discoverable), "CheckDiscovered")]
internal static class HediffComp_Discoverable_CheckDiscovered_Patch
{
    private static void Prefix(bool ___discovered, out bool __state)
    {
        __state = ___discovered;
    }

    private static void Postfix(HediffComp_Discoverable __instance, bool __state, bool ___discovered)
    {
        if (__state || !___discovered)
            return;

        var hediff = __instance.parent;

        GameEventBus.Publish(new HediffDiscoveredEvent(hediff.pawn, hediff, hediff.Part));
    }
}
