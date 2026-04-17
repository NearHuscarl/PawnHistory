using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record LongRangeMineralFoundEvent(Pawn Pawn, ThingDef Material) : GameEventBase;

internal static class LongRangeMineralFoundContext
{
    public static int QuestCountBefore;
}

[HarmonyPatch(typeof(CompLongRangeMineralScanner), "DoFind")]
internal static class CompLongRangeMineralScanner_DoFind_Patch
{
    public static void Prefix()
    {
        LongRangeMineralFoundContext.QuestCountBefore = Find.QuestManager?.QuestsListForReading?.Count ?? 0;
    }

    public static void Postfix(CompLongRangeMineralScanner __instance, Pawn worker)
    {
        if (worker == null)
            return;

        var questCount = Find.QuestManager?.QuestsListForReading?.Count ?? 0;
        if (questCount <= LongRangeMineralFoundContext.QuestCountBefore)
            return;

        var material = Accessor.CompLongRangeMineralScanner.TargetMineable(__instance)?.building?.mineableThing;
        if (material == null)
            return;

        GameEventBus.Publish(new LongRangeMineralFoundEvent(worker, material));
    }
}
