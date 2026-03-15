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
using Verse.AI.Group;
using static RimWorld.PsychicRitualRoleDef;

namespace PawnHistory.Source.PawnTracker.Harmony;

[HarmonyPatch(typeof(MentalState), nameof(MentalState.RecoverFromState))]
static class RecoverFromState_RecoverFromState_Patch
{
    static void Postfix(MentalState __instance)
    {
        GameEventListener.Publish(new MentalStateEndedEvent(__instance.pawn, __instance));
    }
}