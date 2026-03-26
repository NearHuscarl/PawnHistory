using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public class WandererJoinEvent(Pawn pawn, IncidentDef incidentDef) : GameEventBase
{
    public Pawn Pawn { get; } = pawn;
    public IncidentDef IncidentDef { get; } = incidentDef;
}

[HarmonyPatch(typeof(IncidentWorker_WandererJoin), nameof(IncidentWorker_WandererJoin.GeneratePawn))]
public static class IncidentWorker_WandererJoin_GeneratePawn_Patch
{
    public static void Postfix(Pawn __result, IncidentWorker_WandererJoin __instance)
    {
        GameEventBus.Publish(new WandererJoinEvent(__result, __instance.def));
    }
}
