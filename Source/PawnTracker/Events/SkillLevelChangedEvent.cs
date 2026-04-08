using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record SkillLevelChangedEvent(Pawn Pawn, SkillDef Def, int OldLevel, int NewLevel) : GameEventBase;

[HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Learn))]
internal static class SkillRecord_Learn_Patch
{
    static void Prefix(SkillRecord __instance, ref int __state)
    {
        __state = __instance.Level;
    }

    static void Postfix(SkillRecord __instance, int __state)
    {
        var newLevel = __instance.Level;
        var oldLevel = __state;

        if (newLevel == oldLevel) return;

        var pawn = __instance.Pawn;

        GameEventBus.Publish(new SkillLevelChangedEvent(pawn, __instance.def, oldLevel, newLevel));
    }
}
