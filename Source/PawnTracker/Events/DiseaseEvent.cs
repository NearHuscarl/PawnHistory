using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record DiseaseEvent(Pawn Pawn, IEnumerable<Pawn> Group, IncidentDef IncidentDef, BodyPartRecord BodyPart) : GameEventBase;

internal class DiseaseContext
{
    public List<Hediff> HediffSnapshot;

    public static readonly Dictionary<Pawn, DiseaseContext> Contexts = [];

    public static List<Hediff> GetHediffSnapshot(Pawn pawn) => pawn.health.hediffSet.hediffs.ToList();
}

[HarmonyPatch(typeof(IncidentWorker_Disease), nameof(IncidentWorker_Disease.ApplyToPawns))]
internal static class IncidentWorker_Disease_ApplyToPawns_Patch
{
    private static void Prefix(IEnumerable<Pawn> pawns)
    {
        foreach (var pawn in pawns)
        {
            DiseaseContext.Contexts.Add(pawn, new DiseaseContext
            {
                HediffSnapshot = DiseaseContext.GetHediffSnapshot(pawn)
            });
        }
    }
    private static void Postfix(IncidentWorker_Disease __instance, IEnumerable<Pawn> pawns)
    {
        var group = pawns.ToList();
        
        if (group.Count == 0)
            return;
        
        foreach (var pawn in group)
        {
            if (!DiseaseContext.Contexts.TryGetValue(pawn, out var context))
                continue;

            var hediff = DiseaseContext.GetHediffSnapshot(pawn).Except(context.HediffSnapshot).FirstOrDefault();
            GameEventBus.Publish(new DiseaseEvent(pawn, group, __instance.def, hediff?.Part));
        }
    }

    private static void Finalizer() => DiseaseContext.Contexts.Clear();
}
