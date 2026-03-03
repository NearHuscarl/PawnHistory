using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnHistory.Source.PawnTracker
{
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "GenerateRaidLoot")]
    public static class IncidentWorker_RaidEnemy_Patch
    {
        public static void Prefix(IncidentParms parms, float raidLootPoints, List<Pawn> pawns)
        {
            Log.Message("Raided!");
            var raidingPawns = pawns.Where(p => p != null).ToList();
            if (!raidingPawns.Any())
                return;

            GameEventListener.Publish(new RaidEvent(raidingPawns, raidingPawns[0].Faction));
            //GetComp().currentRaid = newRaid;
        }
    }
}
