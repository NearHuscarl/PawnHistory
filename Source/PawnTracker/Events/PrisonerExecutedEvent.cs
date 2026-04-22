using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public record PrisonerExecutedEvent(Pawn Victim, Pawn Executioner, bool Guilty) : GameEventBase;

[HarmonyPatch(typeof(JobDriver_Execute), "MakeNewToils")]
internal static class JobDriver_Execute_MakeNewToils_Patch
{
    private static void Postfix(JobDriver_Execute __instance, ref IEnumerable<Toil> __result)
    {
        var toils = __result.ToList();
        var executionToil = toils.Last();
        var originalAction = executionToil.initAction;

        executionToil.initAction = () =>
        {
            var victim = Accessor.JobDriver_Execute.Victim(__instance);
            if (victim is { IsPrisonerOfColony: true })
                GameEventBus.Publish(new PrisonerExecutedEvent(victim, executionToil.actor, victim.guilt?.IsGuilty ?? false));

            originalAction();
        };

        __result = toils;
    }
}
