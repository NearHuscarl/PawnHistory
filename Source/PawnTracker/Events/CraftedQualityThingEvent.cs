using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record CraftedQualityThingEvent(Pawn Pawn, Thing CraftedThing, QualityCategory Quality) : GameEventBase;

[HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.SendCraftNotification))]
internal static class QualityUtility_SendCraftNotification_Patch
{
    public static void Postfix(Thing thing, Pawn worker)
    {
        if (worker == null || (thing.def.category != ThingCategory.Item && thing.def.category != ThingCategory.Building))
            return;

        if (thing is not ThingWithComps thingWithComps)
            return;

        var qualityComp = thingWithComps.TryGetComp<CompQuality>();
        if (qualityComp == null)
            return;

        var quality = qualityComp.Quality;
        if (quality is not (QualityCategory.Masterwork or QualityCategory.Legendary))
            return;

        GameEventBus.Publish(new CraftedQualityThingEvent(worker, thing, quality));
    }
}
