using HarmonyLib;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public class JobStartedEvent(Pawn pawn, Job oldJob, Job newJob) : GameEventBase
{
    public Pawn Pawn { get; } = pawn;
    public Job OldJob { get; } = oldJob;
    public Job NewJob { get; } = newJob;
}

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
public static class Pawn_JobTracker_StartJob_Patch
{
    static readonly AccessTools.FieldRef<Pawn_JobTracker, Pawn> PawnRef = AccessTools.FieldRefAccess<Pawn_JobTracker, Pawn>("pawn");

    public static void Postfix(Pawn_JobTracker __instance, Job newJob)
    {
        var pawn = PawnRef(__instance);
        var oldJob = __instance.curJob;

        GameEventBus.Publish(new JobStartedEvent(pawn, oldJob, newJob));
    }
}
