using HarmonyLib;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record PawnGeneratedEvent(Pawn Pawn) : GameEventBase;

[HarmonyPatch(typeof(PawnGenerator), "TryGenerateNewPawnInternal")]
[HarmonyPriority(Priority.Last)]
internal static class PawnGenerator_TryGenerateNewPawnInternal_Patch
{
    private static void Postfix(Pawn __result)
    {
        if (__result == null)
            return;

        GameEventBus.Publish(new PawnGeneratedEvent(__result));
    }
}
