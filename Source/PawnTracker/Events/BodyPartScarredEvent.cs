using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum ScarReason
{
    InstantDamage,
    PostHeal,
    Scarification,
}

public record BodyPartScarredEvent(Pawn Pawn, Hediff Hediff, BodyPartRecord Part, Thing Instigator, ScarReason Reason) : GameEventBase;

public class HediffComp_History : HediffComp
{
    public Thing instigator;

    public static void InjectComp()
    {
        foreach (var def in DefDatabase<HediffDef>.AllDefs)
        {
            if (def.HasComp(typeof(HediffComp_GetsPermanent)))
            {
                def.comps.Add(new HediffCompProperties { compClass = typeof(HediffComp_History) });
            }
        }
    }

    public override void CompExposeData()
    {
        Scribe_References.Look(ref instigator, "PH_instigator");
    }
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
    private static void Postfix(Pawn pawn, Hediff_Injury injury, DamageInfo dinfo)
    {
        if (!injury.IsPermanent())
        {
            if (dinfo.Instigator != null && injury.TryGetComp(out HediffComp_History comp))
            {
                comp.instigator = dinfo.Instigator;
            }
            return;
        }

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
    private static void Postfix(HediffComp_GetsPermanent __instance)
    {
        if (!__instance.IsPermanent) return;

        var pawn = __instance.Pawn;
        var hediff = __instance.parent;
        var part = __instance.parent.Part;
        var instigator = hediff.TryGetComp<HediffComp_History>()?.instigator;

        GameEventBus.Publish(new BodyPartScarredEvent(pawn, hediff, part, instigator, ScarReason.PostHeal));
    }
}

// Fired when a pawn is scarified via ritual/job
[HarmonyPatch(typeof(JobDriver_Scarify), nameof(JobDriver_Scarify.Scarify))]
internal class JobDriver_Scarify_Scarify_Patch
{
    private static void Postfix(Pawn pawn, BodyPartRecord part)
    {
        var hediff = pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.ageTicks == 0 && h.def == HediffDefOf.Scarification);
        if (hediff == null) return;

        GameEventBus.Publish(new BodyPartScarredEvent(pawn, hediff, part, null, ScarReason.Scarification));
    }
}
