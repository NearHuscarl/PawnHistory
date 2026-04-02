using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public class DiseaseEvent(Pawn pawn, IEnumerable<Pawn> group, IncidentDef incidentDef, BodyPartRecord bodyPart) : GameEventBase
{
    public Pawn Pawn { get; } = pawn;
    public IEnumerable<Pawn> Group { get; } = group;
    public IncidentDef IncidentDef { get; } = incidentDef;
    public BodyPartRecord BodyPart { get; } = bodyPart;
}

internal class DiseaseContext
{
    public List<Hediff> hediffSnapshot;
    public Pawn pawn;

    public static readonly Dictionary<Pawn, DiseaseContext> Contexts = [];

    public static List<Hediff> GetHediffSnapshot(Pawn pawn) => pawn.health.hediffSet.hediffs.ToList();
}

[HarmonyPatch(typeof(IncidentWorker_Disease), nameof(IncidentWorker_Disease.ApplyToPawns))]
public static class IncidentWorker_Disease_ApplyToPawns_Patch
{
    public static void Prefix(IncidentWorker_Disease __instance, IEnumerable<Pawn> pawns, string blockedInfo)
    {
        foreach (var pawn in pawns)
        {
            DiseaseContext.Contexts.Add(pawn, new DiseaseContext()
            {
                pawn = pawn,
                hediffSnapshot = DiseaseContext.GetHediffSnapshot(pawn)
            });
        }
    }
    public static void Postfix(IncidentWorker_Disease __instance, IEnumerable<Pawn> pawns, string blockedInfo)
    {
        if (!pawns.Any() && blockedInfo.NullOrEmpty())
            return;

        var group = pawns.ToList();
        foreach (var pawn in pawns)
        {
            if (!DiseaseContext.Contexts.TryGetValue(pawn, out var context))
                continue;

            var hediff = DiseaseContext.GetHediffSnapshot(pawn).Except(context.hediffSnapshot).FirstOrDefault();
            GameEventBus.Publish(new DiseaseEvent(pawn, group, __instance.def, hediff?.Part));
        }
    }

    static void Finalizer() => DiseaseContext.Contexts.Clear();
}
