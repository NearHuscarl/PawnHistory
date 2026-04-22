using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public record PrisonerReleasedEvent(Pawn Prisoner, Pawn Releaser) : GameEventBase;

[HarmonyPatch(typeof(JobDriver_ReleasePrisoner), "MakeNewToils")]
internal static class JobDriver_ReleasePrisoner_MakeNewToils_Patch
{
    private static void Postfix(JobDriver_ReleasePrisoner __instance, ref IEnumerable<Toil> __result)
    {
        var toils = __result.ToList();
        var releaseToil = toils.Last();
        var originalAction = releaseToil.initAction;

        releaseToil.initAction = () =>
        {
            originalAction();

            var prisoner = Accessor.JobDriver_ReleasePrisoner.Prisoner(__instance);

            GameEventBus.Publish(new PrisonerReleasedEvent(prisoner, releaseToil.actor));
        };

        __result = toils;
    }
}
