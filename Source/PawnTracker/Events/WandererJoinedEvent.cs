using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record WandererJoinedEvent(Pawn Pawn, IEnumerable<Pawn> Group, IncidentDef IncidentDef = null, QuestScriptDef QuestScript = null) : GameEventBase;

internal record WandererJoinState(List<Pawn> PawnsBefore);

[HarmonyPatch]
internal static class IncidentWorker_TryExecuteWorker_Patch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(IncidentWorker_WandererJoin), "TryExecuteWorker"); // man in black
        yield return AccessTools.Method(typeof(IncidentWorker_GameEndedWanderersJoin), "TryExecuteWorker");
        yield return AccessTools.Method(typeof(IncidentWorker_WildManWandersIn), "TryExecuteWorker");
    }
    
    private static void Prefix(IncidentParms parms, out WandererJoinState __state)
    {
        var map = (Map)parms.target;
        var pawns = map.mapPawns.AllPawnsSpawned.ToList();
        __state = new WandererJoinState(pawns);
    }

    private static void Postfix(IncidentParms parms, IncidentWorker __instance, WandererJoinState __state, bool __result)
    {
        if (!__result)
            return;

        var map = (Map)parms.target;
        var pawns = map.mapPawns.AllPawnsSpawned.ToList();
        var newPawns = pawns.Except(__state.PawnsBefore).ToList();

        foreach (var pawn in newPawns)
        {
            GameEventBus.Publish(new WandererJoinedEvent(pawn, newPawns, __instance.def));
        }
    }
}
