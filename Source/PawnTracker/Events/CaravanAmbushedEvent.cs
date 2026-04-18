using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record CaravanAmbushedEvent(List<Pawn> Enemies, Faction Faction, string Caravan) : GameEventBase;

[HarmonyPatch(typeof(IncidentWorker_Ambush), "DoExecute")]
internal static class IncidentWorker_Ambush_DoExecute_Patch
{
    private static void Postfix(bool __result, IncidentWorker_Ambush __instance, IncidentParms parms, List<Pawn> generatedEnemies)
    {
        if (!__result || parms.target is not Caravan caravan)
            return;

        GameEventBus.Publish(new CaravanAmbushedEvent(generatedEnemies.ToList(), parms.faction, caravan.Label));
    }
}
