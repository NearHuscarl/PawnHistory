using HarmonyLib;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public class JobEndEvent(Pawn pawn, Job currentJob, JobCondition condition) : GameEventBase
{
    public Pawn Pawn { get; } = pawn;
    public Job CurrentJob { get; } = currentJob;
    public JobCondition Condition { get; } = condition;
}

[HarmonyPatch(typeof(Pawn_JobTracker), "CleanupCurrentJob")]
public static class Pawn_JobTracker_CleanupCurrentJob_Patch
{
    public static void Prefix(Pawn_JobTracker __instance, JobCondition condition)
    {
        var pawn = JobStartedContext.PawnRef(__instance);
        var curJob = __instance.curJob;

        GameEventBus.Publish(new JobEndEvent(pawn, curJob, condition));
    }
}
