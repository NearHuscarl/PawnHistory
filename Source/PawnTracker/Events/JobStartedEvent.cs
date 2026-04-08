using HarmonyLib;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public record JobStartedEvent(Pawn Pawn, Job OldJob, Job NewJob) : GameEventBase;

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
public static class Pawn_JobTracker_StartJob_Patch
{

    public static void Postfix(Pawn_JobTracker __instance, Job newJob)
    {
        var pawn = Accessor.Pawn_JobTracker.Pawn(__instance);
        var oldJob = __instance.curJob;

        GameEventBus.Publish(new JobStartedEvent(pawn, oldJob, newJob));
    }
}
