using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public record PrisonerEscapedEvent(Pawn Prisoner) : GameEventBase;

[HarmonyPatch(typeof(JobGiver_PrisonerEscape), "TryGiveJob")]
internal static class JobGiver_PrisonerEscape_TryGiveJob_Patch
{
    private static void Postfix(Pawn pawn, Job __result)
    {
        if (__result == null || pawn.guest.Released)
            return;

        GameEventBus.Publish(new PrisonerEscapedEvent(pawn));
    }
}
