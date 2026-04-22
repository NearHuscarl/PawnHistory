using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record WandererJoinedEvent(Pawn Pawn, IEnumerable<Pawn> Group, IncidentDef IncidentDef = null, QuestScriptDef QuestScript = null) : GameEventBase;

internal static class WandererJoinContext
{
    private static List<Pawn> snapshot = [];

    public static IEnumerable<Pawn> UpdatePawnSnapshot(Map map)
    {
        var newSnapshot = map.mapPawns.AllPawnsSpawned.ToList();
        var oldSnapshot = snapshot;
        var difference = newSnapshot.Except(oldSnapshot);
        
        snapshot = newSnapshot;
        return difference;
    }

    public static void Prefix(IncidentParms parms)
    {
        var map = (Map)parms.target;
        UpdatePawnSnapshot(map);
    }

    public static void Postfix(IncidentParms parms, IncidentWorker __instance, bool __result)
    {
        if (!__result)
            return;

        var map = (Map)parms.target;
        var pawns = UpdatePawnSnapshot(map);

        foreach (var pawn in pawns)
        {
            GameEventBus.Publish(new WandererJoinedEvent(pawn, pawns, __instance.def));
        }
    }

    internal static void Finalizer()
    {
        snapshot.Clear();
    }
}

[HarmonyPatch]
internal static class IncidentWorker_TryExecuteWorker_Patch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(IncidentWorker_WandererJoin), "TryExecuteWorker"); // man in black
        yield return AccessTools.Method(typeof(IncidentWorker_GameEndedWanderersJoin), "TryExecuteWorker");
        yield return AccessTools.Method(typeof(IncidentWorker_WildManWandersIn), "TryExecuteWorker");
    }
    
    private static void Prefix(IncidentParms parms) => WandererJoinContext.Prefix(parms);

    private static void Postfix(IncidentParms parms, IncidentWorker __instance, bool __result)
    {
        WandererJoinContext.Postfix(parms, __instance, __result);
    }

    private static void Finalizer() => WandererJoinContext.Finalizer();
}
