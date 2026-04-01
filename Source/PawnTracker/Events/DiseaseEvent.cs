using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public class DiseaseEvent(Pawn pawn, IEnumerable<Pawn> group, IncidentDef incidentDef) : GameEventBase
{
    public Pawn Pawn { get; } = pawn;
    public IEnumerable<Pawn> Group { get; } = group;
    public IncidentDef IncidentDef { get; } = incidentDef;
}

[HarmonyPatch(typeof(IncidentWorker_Disease), nameof(IncidentWorker_Disease.ApplyToPawns))]
public static class IncidentWorker_Disease_ApplyToPawns_Patch
{
    public static void Postfix(IncidentWorker_Disease __instance, IEnumerable<Pawn> pawns, string blockedInfo)
    {
        if (!pawns.Any() && blockedInfo.NullOrEmpty())
            return;

        var group = pawns.ToList();
        foreach (var pawn in pawns)
        {
            GameEventBus.Publish(new DiseaseEvent(pawn, group, __instance.def));
        }
    }
}
