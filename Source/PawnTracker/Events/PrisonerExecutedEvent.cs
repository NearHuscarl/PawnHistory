using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public enum PrisonerExecutionRoute
{
    Prisoner,
    GuiltyColonist,
}

public record PrisonerExecutedEvent(Pawn Victim, Pawn Executioner, bool Guilty, PrisonerExecutionRoute ExecutionRoute) : GameEventBase;

internal static class PrisonerExecutedContext
{
    internal static void WrapExecutionToil<TDriver>(TDriver __instance, ref IEnumerable<Toil> __result, Func<TDriver, Pawn> getVictim, PrisonerExecutionRoute route) where TDriver : JobDriver
    {
        var toils = __result.ToList();
        var executionToil = toils.Last();
        var originalAction = executionToil.initAction;

        executionToil.initAction = () =>
        {
            var victim = getVictim(__instance);
            var isGuilty = victim?.guilt?.IsGuilty ?? false;
            var executioner = executionToil.actor;
            GameEventBus.Publish(new PrisonerExecutedEvent(victim, executioner, isGuilty, route));
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
        PrisonerExecutedContext.WrapExecutionToil(__instance, ref __result, Accessor.JobDriver_Execute.Victim, PrisonerExecutionRoute.Prisoner);
    }
}

[HarmonyPatch(typeof(JobDriver_ExecuteGuiltyColonist), "MakeNewToils")]
internal static class JobDriver_ExecuteGuiltyColonist_MakeNewToils_Patch
{
    private static void Postfix(JobDriver_ExecuteGuiltyColonist __instance, ref IEnumerable<Toil> __result)
    {
        PrisonerExecutedContext.WrapExecutionToil(__instance, ref __result, Accessor.JobDriver_ExecuteGuiltyColonist.Victim, PrisonerExecutionRoute.GuiltyColonist);
    }
}
