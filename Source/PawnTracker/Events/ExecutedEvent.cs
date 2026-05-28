using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public enum ExecutionRoute
{
    Prisoner,
    GuiltyColonist,
    Slave,
}

public record ExecutedEvent(Pawn Victim, Pawn Executioner, bool Guilty, ExecutionRoute Route) : GameEventBase;

file static class ExecutedContext
{
    internal static void WrapExecutionToil<TDriver>(TDriver __instance, ref IEnumerable<Toil> __result, Func<TDriver, Pawn> getVictim, ExecutionRoute route) where TDriver : JobDriver
    {
        var toils = __result.ToList();
        var executionToil = toils.Last();
        var originalAction = executionToil.initAction;

        executionToil.initAction = () =>
        {
            var victim = getVictim(__instance);
            var executioner = executionToil.actor;
            var isGuilty = victim?.guilt?.IsGuilty ?? false;
            GameEventBus.Publish(new ExecutedEvent(victim, executioner, isGuilty, route));
            originalAction();
        };

        __result = toils;
    }
}

[HarmonyPatch(typeof(JobDriver_Execute), "MakeNewToils")]
internal static class JobDriver_Execute_MakeNewToils_Patch
{
    private static void Postfix(JobDriver_Execute __instance, ref IEnumerable<Toil> __result)
    {
        ExecutedContext.WrapExecutionToil(__instance, ref __result, Accessor.JobDriver_Execute.Victim, ExecutionRoute.Prisoner);
    }
}

[HarmonyPatch(typeof(JobDriver_ExecuteGuiltyColonist), "MakeNewToils")]
internal static class JobDriver_ExecuteGuiltyColonist_MakeNewToils_Patch
{
    private static void Postfix(JobDriver_ExecuteGuiltyColonist __instance, ref IEnumerable<Toil> __result)
    {
        ExecutedContext.WrapExecutionToil(__instance, ref __result, Accessor.JobDriver_ExecuteGuiltyColonist.Victim, ExecutionRoute.GuiltyColonist);
    }
}

[HarmonyPatch(typeof(JobDriver_ExecuteSlave), "MakeNewToils")]
internal static class JobDriver_ExecuteSlave_MakeNewToils_Patch
{
    private static void Postfix(JobDriver_ExecuteSlave __instance, ref IEnumerable<Toil> __result)
    {
        ExecutedContext.WrapExecutionToil(__instance, ref __result, Accessor.JobDriver_ExecuteSlave.Victim, ExecutionRoute.Slave);
    }
}
