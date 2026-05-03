using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

/// <summary>
/// </summary>
/// <param name="Carrier"></param>
/// <param name="Mother">is null if the pawn containing the hediff is the mother. not null if they are only the carrier, not the biological mother</param>
/// <param name="Father">can be null if trigger via debug action</param>
/// <param name="Pregnancy"></param>
public record PregnancyStartedEvent(Pawn Carrier, Pawn Mother, Pawn Father) : GameEventBase;

[HarmonyPatch(typeof(HediffComp_MessageAfterTicks), nameof(HediffComp_MessageAfterTicks.CompPostTick))]
internal static class HediffComp_MessageAfterTicks_CompPostTick_Patch
{
    private static void Prefix(HediffComp_MessageAfterTicks __instance)
    {
        if (Accessor.HediffComp_MessageAfterTicks.TicksUntilMessage(__instance) != 0)
            return;

        var hediff = __instance.parent;
        if (hediff.def != HediffDefOf.PregnantHuman || hediff is not Hediff_Pregnant pregnancy)
            return;

        GameEventBus.Publish(new PregnancyStartedEvent(pregnancy.pawn, pregnancy.Mother, pregnancy.Father));
    }
}
