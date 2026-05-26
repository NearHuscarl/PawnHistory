using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public abstract record SurgeryEventData;

public record SurgeryEvent(RecipeDef Recipe, Pawn Patient, Pawn Doctor, BodyPartRecord Part, SurgeryEventData Data) : GameEventBase
{
    public List<Hediff> NewInjuries { get; set; }
    public SurgeryOutcome Outcome { get; set; }
}

internal abstract class SurgeryEventDataSource
{
    private static readonly Lazy<Dictionary<string, SurgeryEventDataSource>> SourceLookup = new(BuildSources);

    protected abstract Type GetWorkClass();
    protected abstract SurgeryEventData Create(RecipeDef recipe, Pawn patient, Pawn doctor, BodyPartRecord part);

    public static SurgeryEvent CreateEvent(RecipeDef recipe, Pawn patient, Pawn doctor, BodyPartRecord part)
    {
        SurgeryEventData data = null;
        
        if (SourceLookup.Value.TryGetValue(recipe.workerClass.Name, out var source))
            data = source.Create(recipe, patient, doctor, part);

        return new SurgeryEvent(recipe, patient, doctor, part, data);
    }

    private static Dictionary<string, SurgeryEventDataSource> BuildSources()
    {
        return typeof(SurgeryEventDataSource)
            .AllSubclassesNonAbstract()
            .Select(t => (SurgeryEventDataSource)Activator.CreateInstance(t))
            .ToDictionary(d => d.GetWorkClass().Name, d => d);
    }
}

file class SurgeryContext(SurgeryEvent e)
{
    public static SurgeryContext Frame;

    public SurgeryEvent Event { get; } = e;
    private List<Hediff> InjurySnapshot { get; } = GetInjuries(e.Patient);

    public List<Hediff> NewInjuries()
    {
        // Injury hediffs are those added during the failed surgery (e.g. surgical cut).
        return GetInjuries(Event.Patient)
            .Except(InjurySnapshot)
            .OrderByDescending(h => h.Severity)
            .ToList();
    }

    private static List<Hediff> GetInjuries(Pawn pawn)
    {
        return pawn.health.hediffSet.hediffs.Where(h => h is Hediff_Injury).ToList();
    }
}

// Call order:
// Recipe_Surgery.ApplyOnPawn() prefix
// SurgeryOutcomeEffectDef.GetOutcome()
// Recipe_Surgery.ApplyOnPawn() postfix
[HarmonyPatch]
internal class Recipe_Surgery_ApplyOnPawn_Patch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Recipe_InstallImplant), nameof(Recipe_Surgery.ApplyOnPawn));
        yield return AccessTools.Method(typeof(Recipe_InstallNaturalBodyPart), nameof(Recipe_Surgery.ApplyOnPawn));
        yield return AccessTools.Method(typeof(Recipe_InstallArtificialBodyPart), nameof(Recipe_Surgery.ApplyOnPawn));
        yield return AccessTools.Method(typeof(Recipe_RemoveBodyPart), nameof(Recipe_Surgery.ApplyOnPawn));
        yield return AccessTools.Method(typeof(Recipe_AddHediff), nameof(Recipe_Surgery.ApplyOnPawn));
        yield return AccessTools.Method(typeof(Recipe_ImplantIUD), nameof(Recipe_Surgery.ApplyOnPawn));
        yield return AccessTools.Method(typeof(Recipe_RemoveHediff), nameof(Recipe_Surgery.ApplyOnPawn));
        yield return AccessTools.Method(typeof(Recipe_TerminatePregnancy), nameof(Recipe_Surgery.ApplyOnPawn));
    }

    private static void Prefix(Recipe_Surgery __instance, Pawn pawn, BodyPartRecord part, Pawn billDoer)
    {
        if (billDoer == null) // not surgery related
            return;

        var e = SurgeryEventDataSource.CreateEvent(__instance.recipe, pawn, billDoer, part);
        SurgeryContext.Frame = new SurgeryContext(e);
    }

    private static void Postfix()
    {
        if (SurgeryContext.Frame == null)
            return;

        SurgeryContext.Frame.Event.NewInjuries = SurgeryContext.Frame.NewInjuries();
        GameEventBus.Publish(SurgeryContext.Frame.Event);
    }

    private static void Finalizer()
    {
        SurgeryContext.Frame = null;
    }
}

[HarmonyPatch(typeof(SurgeryOutcomeEffectDef), nameof(SurgeryOutcomeEffectDef.GetOutcome))]
internal class SurgeryOutcomeEffectDef_GetOutcome_Patch
{
    private static void Postfix(Pawn patient, SurgeryOutcome __result)
    {
        if (SurgeryContext.Frame?.Event.Patient == patient)
            SurgeryContext.Frame?.Event.Outcome = __result;
    }
}
