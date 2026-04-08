using HarmonyLib;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public record JobEndEvent(Pawn Pawn, Job CurrentJob, JobCondition Condition) : GameEventBase;

[HarmonyPatch(typeof(Pawn_JobTracker), "CleanupCurrentJob")]
public static class Pawn_JobTracker_CleanupCurrentJob_Patch
{
    public static void Prefix(Pawn_JobTracker __instance, JobCondition condition)
    {
        var pawn = Accessor.Pawn_JobTracker.Pawn(__instance);
        var curJob = __instance.curJob;

        GameEventBus.Publish(new JobEndEvent(pawn, curJob, condition));
    }
}
