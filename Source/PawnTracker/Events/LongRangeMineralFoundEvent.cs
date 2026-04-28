using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record LongRangeMineralFoundEvent(Pawn Pawn, ThingDef Material) : GameEventBase;

internal record LongRangeMineralFoundState(int QuestCountBefore);

[HarmonyPatch(typeof(CompLongRangeMineralScanner), "DoFind")]
internal static class CompLongRangeMineralScanner_DoFind_Patch
{
    public static void Prefix(out LongRangeMineralFoundState __state)
    {
        __state = new LongRangeMineralFoundState(Find.QuestManager.QuestsListForReading.Count);
    }

    public static void Postfix(CompLongRangeMineralScanner __instance, LongRangeMineralFoundState __state, Pawn worker)
    {
        if (worker == null)
            return;

        var questCount = Find.QuestManager.QuestsListForReading.Count;
        if (questCount <= __state.QuestCountBefore)
            return;

        var material = Accessor.CompLongRangeMineralScanner.TargetMineable(__instance).building.mineableThing;
        if (material == null)
            return;

        GameEventBus.Publish(new LongRangeMineralFoundEvent(worker, material));
    }
}
