using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record SurgeryEvent(Pawn Patient, Pawn Doctor, BodyPartRecord Part, List<Hediff> NewInjuries = null, SurgeryOutcome Outcome = null) : GameEventBase;

// Call order:
// Recipe_Surgery.ApplyOnPawn() prefix
// SurgeryOutcomeEffectDef.GetOutcome()
// Recipe_Surgery.ApplyOnPawn() postfix

internal class SurgeryContext<T> where T : SurgeryEvent
{
    public T e;
    public List<Hediff> InjurySnapshot;

    public static readonly Dictionary<string, SurgeryContext<T>> PendingSurgeries = [];

    public static List<Hediff> GetInjurySnapshot(Pawn pawn) => pawn.health.hediffSet.hediffs.Where(h => h is Hediff_Injury).ToList();

    public static void SurgeryRecipe_PreApplyOnPawn(Pawn patient, Func<T> eventFactory)
    {
        PendingSurgeries[GetSurgeryId(patient)] = new SurgeryContext<T>
        {
            e = eventFactory(),
            InjurySnapshot = GetInjurySnapshot(patient),
        };
    }

    public static void SurgeryRecipe_PostApplyOnPawn(Pawn patient)
    {
        var surgeryId = GetSurgeryId(patient);
        if (!PendingSurgeries.Remove(surgeryId, out var ctx))
            return;

        // Injury hediffs are those added to the part during the failed surgery
        // (e.g. surgical cut, etc.) - compare snapshot to current state
        var newInjuries = GetInjurySnapshot(patient)
            .Except(ctx.InjurySnapshot)
            .OrderByDescending(h => h.Severity)
            .ToList();
        ctx.e = ctx.e with { NewInjuries = newInjuries };

        GameEventBus.Publish(ctx.e);
    }

    public static void SurgeryOutcomeEffectDef_PostGetOutcome(Pawn patient, SurgeryOutcome __result)
    {
        if (!PendingSurgeries.TryGetValue(GetSurgeryId(patient), out var ctx))
            return;
        ctx.e = ctx.e with { Outcome = __result };
    }

    public static string GetSurgeryId(Pawn pawn)
    {
        var surgeryEventType = typeof(T);
        var surgeryId = $"{pawn.GetUniqueLoadID()}_{surgeryEventType.Name}";
        return surgeryId;
    }
}
