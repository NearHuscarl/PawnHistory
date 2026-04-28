using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record DiseaseEvent(Pawn Pawn, IEnumerable<Pawn> Group, IncidentDef IncidentDef, BodyPartRecord BodyPart) : GameEventBase;

internal static class DiseaseContext
{
    public static List<Hediff> GetHediffSnapshot(Pawn pawn) => pawn.health.hediffSet.hediffs.ToList();
}

internal class DiseaseState
{
    public readonly Dictionary<Pawn, List<Hediff>> HediffSnapshots = [];
}

[HarmonyPatch(typeof(IncidentWorker_Disease), nameof(IncidentWorker_Disease.ApplyToPawns))]
internal static class IncidentWorker_Disease_ApplyToPawns_Patch
{
    private static void Prefix(IEnumerable<Pawn> pawns, out DiseaseState __state)
    {
        __state = new DiseaseState();
        foreach (var pawn in pawns)
        {
            __state.HediffSnapshots.Add(pawn, DiseaseContext.GetHediffSnapshot(pawn));
        }
    }
    private static void Postfix(IncidentWorker_Disease __instance, DiseaseState __state, IEnumerable<Pawn> pawns)
    {
        var group = pawns.ToList();
        
        if (group.Count == 0)
            return;
        
        foreach (var pawn in group)
        {
            if (!__state.HediffSnapshots.TryGetValue(pawn, out var snapshot))
                continue;

            var hediff = DiseaseContext.GetHediffSnapshot(pawn).Except(snapshot).FirstOrDefault();
            GameEventBus.Publish(new DiseaseEvent(pawn, group, __instance.def, hediff?.Part));
        }
    }
}
