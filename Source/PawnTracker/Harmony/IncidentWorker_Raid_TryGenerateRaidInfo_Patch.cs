using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace PawnHistory.Source.PawnTracker.Harmony;

[HarmonyPatch(typeof(IncidentWorker_Raid), nameof(IncidentWorker_Raid.TryGenerateRaidInfo))]
public static class IncidentWorker_Raid_TryGenerateRaidInfo_Patch
{
    public static void Postfix(bool __result, IncidentWorker_Raid __instance, IncidentParms parms, List<Pawn> pawns, bool debugTest = false)
    {
        if (!__result)
            return; // cannot spawn a raid due to internal error

        GameEventListener.Publish(new RaidEvent(pawns, parms.faction, parms.raidStrategy, parms.raidArrivalMode, isFriendly: __instance is IncidentWorker_RaidFriendly));
    }
}
