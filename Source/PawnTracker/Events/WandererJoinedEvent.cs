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
    private static List<Pawn> Snapshot = [];

    public static IEnumerable<Pawn> UpdatePawnSnapshot(Map map)
    {
        var newSnapshot = map.mapPawns.AllPawnsSpawned.ToList();
        var oldSnapshot = Snapshot;
        var difference = newSnapshot.Except(oldSnapshot);
        
        Snapshot = newSnapshot;
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
        Snapshot.Clear();
    }
}

// Wait until the player accepts the ChoiceLetter
[HarmonyPatch(typeof(QuestPart_PawnsArrive), nameof(QuestPart_PawnsArrive.Notify_QuestSignalReceived))]
internal class QuestPart_PawnsArrive_Notify_QuestSignalReceived_Patch
{
    static void Prefix(QuestPart_PawnsArrive __instance, Signal signal)
    {
        if (signal.tag != __instance.inSignal) return;

        WandererJoinContext.UpdatePawnSnapshot(__instance.mapParent.Map);
    }
    static void Postfix(QuestPart_PawnsArrive __instance, Signal signal)
    {
        if (signal.tag != __instance.inSignal) return;

        var newPawns = WandererJoinContext.UpdatePawnSnapshot(__instance.mapParent.Map);

        foreach (var pawn in newPawns)
        {
            GameEventBus.Publish(new WandererJoinedEvent(pawn, newPawns, null, __instance.quest.root));
        }
    }
}

[HarmonyPatch(typeof(QuestNode_Root_RefugeePodCrash), nameof(QuestNode_Root_RefugeePodCrash.GeneratePawn))]
internal class QuestNode_Root_RefugeePodCrash_GeneratePawn_Patch
{
    private static void Postfix(Pawn __result)
    {
        GameEventBus.Publish(new WandererJoinedEvent(__result, [], null, QuestGen.quest.root));
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
