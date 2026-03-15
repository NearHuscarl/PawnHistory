using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using static RimWorld.PsychicRitualRoleDef;

namespace PawnHistory.Source.PawnTracker.Harmony;

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
public static class Pawn_JobTracker_StartJob_Patch
{
    static readonly AccessTools.FieldRef<Pawn_JobTracker, Pawn> PawnRef = AccessTools.FieldRefAccess<Pawn_JobTracker, Pawn>("pawn");

    public static void Postfix(Pawn_JobTracker __instance, Job newJob)
    {
        var pawn = PawnRef(__instance);
        var oldJob = __instance.curJob;

        GameEventListener.Publish(new JobStartedEvent(pawn, oldJob, newJob));
    }
}
