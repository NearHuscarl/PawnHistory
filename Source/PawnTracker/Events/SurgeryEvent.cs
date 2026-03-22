using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

internal class SurgeryEvent(Pawn patient, Pawn doctor, BodyPartRecord part) : GameEventBase
{
    public Pawn Patient { get; } = patient;
    public Pawn Doctor { get; } = doctor;
    public BodyPartRecord Part { get; } = part;
    public List<Hediff> NewInjuries { get; set; }
    public SurgeryOutcome Outcome { get; set; }
}

// Call order:
// Recipe_Surgery.ApplyOnPawn() prefix
// SurgeryOutcomeEffectDef.GetOutcome()
// Recipe_Surgery.ApplyOnPawn() postfix

internal class SurgeryContext<T> where T : SurgeryEvent
{
    public T e;
    public List<Hediff> injurySnapshot;

    public static readonly Dictionary<string, SurgeryContext<T>> PendingSurgeries = [];

    public static List<Hediff> GetInjurySnapshot(Pawn pawn) => pawn.health.hediffSet.hediffs.Where(h => h is Hediff_Injury).ToList();

    public static void SurgeryRecipe_PreApplyOnPawn(Pawn patient, Func<T> eventFactory)
    {
        PendingSurgeries[GetSurgeryId(patient)] = new SurgeryContext<T>()
        {
            e = eventFactory(),
            injurySnapshot = GetInjurySnapshot(patient),
        };
    }

    public static void SurgeryRecipe_PostApplyOnPawn(Pawn patient)
    {
        var surgeryId = GetSurgeryId(patient);
        if (!PendingSurgeries.TryGetValue(surgeryId, out var ctx))
            return;
        PendingSurgeries.Remove(surgeryId);

        // Injury hediffs are those added to the part during the failed surgery
        // (e.g. surgical cut, etc.) - compare snapshot to current state
        ctx.e.NewInjuries = GetInjurySnapshot(patient)
            .Except(ctx.injurySnapshot)
            .OrderByDescending(h => h.Severity)
            .ToList();

        GameEventBus.Publish(ctx.e);
    }

    public static void SurgeryOutcomeEffectDef_PostGetOutcome(Pawn patient, SurgeryOutcome __result)
    {
        if (!PendingSurgeries.TryGetValue(GetSurgeryId(patient), out var ctx))
            return;
        ctx.e.Outcome = __result;
    }

    public static string GetSurgeryId(Pawn pawn)
    {
        var surgeryEventType = typeof(T);
        var surgeryId = $"{pawn.GetUniqueLoadID()}_{surgeryEventType.Name}";
        return surgeryId;
    }
}
