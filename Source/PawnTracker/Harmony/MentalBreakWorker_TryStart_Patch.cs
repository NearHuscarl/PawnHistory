using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Harmony;

[HarmonyPatch]
public static class MentalBreakWorker_TryStart_Patch
{
    static void Prefix(MentalBreakWorker __instance, Pawn pawn, string reason, bool causedByMood)
    {
        GameEventBus.Publish(new MentalBreakStartEvent(pawn, reason, __instance));
    }
    static void Postfix(MentalBreakWorker __instance, bool __result, Pawn pawn, string reason, bool causedByMood)
    {
        if (!__result) return; // break didn't start
        GameEventBus.Publish(new MentalBreakStartedEvent(pawn, reason, __instance));
    }

    static IEnumerable<MethodBase> TargetMethods()
    {
        var baseType = typeof(MentalBreakWorker);

        return baseType.Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t))
            .Select(t => AccessTools.Method(t, nameof(MentalBreakWorker.TryStart)))
            .Where(m => m != null);
    }
}