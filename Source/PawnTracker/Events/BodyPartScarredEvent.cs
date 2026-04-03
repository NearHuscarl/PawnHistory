using HarmonyLib;
using RimWorld;
using System.Runtime.CompilerServices;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum ScarReason
{
    InstantDamage,
    PostHeal,
    Scarification,
}

public class BodyPartScarredEvent(Pawn pawn, Hediff hediff, BodyPartRecord part, Thing instigator, ScarReason reason) : GameEventBase
{
    public Pawn Pawn { get; } = pawn;
    public Hediff Hediff { get; } = hediff;
    public BodyPartRecord Part { get; } = part;
    public Thing Instigator { get; } = instigator;
    public ScarReason Reason { get; } = reason;

}

class BodyPartScarredContext
{
    public static readonly ConditionalWeakTable<Hediff, Thing> InstigatorLookup = [];
}

// Search for: "IsPermanent = true;"

// Call order:
// - DamageWorker_AddInjury.FinalizeAndAddInjury() > PreFinalizeInjury()
// - ...
// - HediffComp_GetsPermanent.CompPostInjuryHeal()

// Instant scar from injury, for delicate body parts like brain/eyes.
[HarmonyPatch(typeof(DamageWorker_AddInjury), "FinalizeAndAddInjury", [typeof(Pawn), typeof(Hediff_Injury), typeof(DamageInfo), typeof(DamageWorker.DamageResult)])]
internal class DamageWorker_AddInjury_FinalizeAndAddInjury_Patch
{
    static void Postfix(Pawn pawn, Hediff_Injury injury, DamageInfo dinfo)
    {
        if (dinfo.Instigator != null && injury.TryGetComp<HediffComp_GetsPermanent>(out _))
            BodyPartScarredContext.InstigatorLookup.GetValue(injury, _ => dinfo.Instigator);

        if (!injury.IsPermanent()) return;

        var part = injury.Part;

        // scarred body part can be destroyed, which removes the scar after AddHediff(): PreAddHediff(Scar) > PreAddHediff(Missing) > PostAddHediff(Missing) > PostAddHediff(Scar)
        if (pawn.health.hediffSet.PartIsMissing(part))
            return;

        GameEventBus.Publish(new BodyPartScarredEvent(pawn, injury, part, dinfo.Instigator, ScarReason.InstantDamage));
    }
}

// Fired after healing when an injury crosses the threshold and becomes permanent (delayed scar).
[HarmonyPatch(typeof(HediffComp_GetsPermanent), nameof(HediffComp_GetsPermanent.CompPostInjuryHeal))]
internal class HediffComp_GetsPermanent_CompPostInjuryHeal_Patch
{
    static void Postfix(HediffComp_GetsPermanent __instance)
    {
        if (!__instance.IsPermanent) return;

        var pawn = __instance.Pawn;
        var hediff = __instance.parent;
        var part = __instance.parent.Part;
        BodyPartScarredContext.InstigatorLookup.TryGetValue(hediff, out var instigator);

        GameEventBus.Publish(new BodyPartScarredEvent(pawn, hediff, part, instigator, ScarReason.PostHeal));
    }
}

// Fired when a pawn is scarified via ritual/job
[HarmonyPatch(typeof(JobDriver_Scarify), nameof(JobDriver_Scarify.Scarify))]
internal class JobDriver_Scarify_Scarify_Patch
{
    static void Postfix(Pawn pawn, BodyPartRecord part)
    {
        var hediff = pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.ageTicks == 0 && h.def == HediffDefOf.Scarification);
        if (hediff == null) return;

        GameEventBus.Publish(new BodyPartScarredEvent(pawn, hediff, part, null, ScarReason.Scarification));
    }
}
