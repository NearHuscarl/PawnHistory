using PawnHistory.Source.PawnTracker.Events;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Verse;
using Verse.Grammar;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class PermanentDamageRecorder : RecorderBase
{
    public override void Register()
    {
        // Mental model:
        // - Normal: PreAddHediff(hediff) > PostAddHediff(hediff)
        // - If a hediff causes missing part: PreAddHediff(SurgicalCut) > PreAddHediff(MissingBodyPart) > PostAddHediff(MissingBodyPart) > PostAddHediff(SurgicalCut)
        // Reference: DamageWorker_AddInjury.FinalizeAndAddInjury()
        GameEventBus.Subscribe<HediffAddEvent>(e =>
        {
            var pawn = e.Pawn;
            var hediff = e.Hediff;
            var part = e.Part;
            var dinfo = e.Dinfo;

            if (!ShouldRecord(pawn))
                return;

            if (part != null && hediff.def == HediffDefOf.MissingBodyPart && dinfo?.Def == DamageDefOf.SurgicalCut)
                HandleSurgicalCutEvent(pawn, hediff, part);
        });

        GameEventBus.Subscribe<HediffAddedEvent>(e =>
        {
            var pawn = e.Pawn;
            var hediff = e.Hediff;
            var part = e.Part;
            var dinfo = e.Dinfo;

            if (!ShouldRecord(pawn))
                return;

            if (part == null)
                return;

            if (hediff.def == HediffDefOf.MissingBodyPart)
            {
                // missing vital body part will make a pawn die, this is handled by in-game combat log instead.
                if (pawn.Dead)
                    return;

                if (dinfo?.Def != DamageDefOf.SurgicalCut)
                    HandleDestroyPartEvent(pawn, hediff, part, dinfo);
            }
            else if (hediff.IsPermanent() && dinfo.HasValue /* from combat rather than old wound */)
            {
                // scarred body part can be destroyed, which removes the scar after AddHediff(): PreAddHediff(Scar) > PreAddHediff(Missing) > PostAddHediff(Missing) > PostAddHediff(Scar)
                if (!pawn.health.hediffSet.PartIsMissing(part))
                    HandleScarredPartEvent(pawn, hediff, part, dinfo);
            }
        });
    }

    // Must be called in PreAdd to retrieve the bad hediff, as MissingBodyPart hediff will remove it at the end of AddHediff().
    private void HandleSurgicalCutEvent(Pawn pawn, Hediff hediff, BodyPartRecord part)
    {
        var recordDef = HistoryRecordDefOf.BodyPartLost;
        var doctor = GetOperatingDoctor(pawn);
        var removeIntent = PartRemovalIntent(pawn, part, out Hediff badHediff);
        var desc = recordDef.ResolveDescription("bodyPartLostSurgery", pawn)
            .AddRule("DOCTOR", doctor)
            .AddRule("PART", part.Label.Colorize(hediff.LabelColor))
            .AddRule("HEDIFF", badHediff)
            .AddConstant("intent", removeIntent)
            .Resolve();
        AddRecord(recordDef, pawn, desc, [doctor]);
    }

    // Must be called in postfix because hediff.label does not exist in prefix.
    private void HandleDestroyPartEvent(Pawn pawn, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo)
    {
        var instigator = dinfo?.Instigator as Pawn;
        var weapon = dinfo?.Weapon?.race != null ? dinfo?.Tool?.label /* body part like fist/teeth */ : dinfo?.Weapon?.label;
        var recordDef = HistoryRecordDefOf.BodyPartLost;
        var descBuilder = recordDef.ResolveDescription("bodyPartLost", pawn)
            .AddRule("PART", part.Label.Colorize(hediff.LabelColor))
            .AddRule("HEDIFF", hediff) // <destroyedLabel>
            .AddRule("WEAPON", weapon)
            .AddConstantIf(weapon != null, "hasWeapon", "true");

        if (dinfo?.Instigator is Pawn)
        {
            descBuilder
                .AddRule("INSTIGATOR", instigator)
                .AddConstant("hasInstigator", "true");
        }

        AddRecord(recordDef, pawn, descBuilder.Resolve(), [instigator]);
    }

    private void HandleScarredPartEvent(Pawn pawn, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo)
    {
        var instigator = dinfo?.Instigator as Pawn;
        var weapon = dinfo?.Weapon?.race != null ? dinfo?.Tool?.label /* body part like fist/teeth */ : dinfo?.Weapon?.label;
        var recordDef = HistoryRecordDefOf.BodyPartPermanentlyDamaged;
        var descBuilder = recordDef.ResolveDescription("bodyPartPermanentlyDamaged", pawn)
            .IncludePawnGrammar()
            .AddRule("PART", part.Label.Colorize(hediff.LabelColor))
            .AddRule("HEDIFF", hediff) // <permanentLabel>
            .AddRule("WEAPON", weapon)
            .AddConstantIf(weapon != null, "hasWeapon", "true");

        if (dinfo?.Instigator is Pawn)
        {
            descBuilder
                .AddRule("INSTIGATOR", instigator)
                .AddConstant("hasInstigator", "true");
        }

        AddRecord(recordDef, pawn, descBuilder.Resolve(), [instigator]);
    }

    public static Pawn GetOperatingDoctor(Pawn patient)
    {
        var comp = patient.CurrentBed()?.GetComp<CompAssignableToPawn_Bed>();
        var medicalBill = patient.BillStack.Bills.OfType<Bill_Medical>().FirstOrDefault();

        if (medicalBill == null) return null;

        return patient.Map.mapPawns.AllPawnsSpawned
            .FirstOrDefault(p => p.CurJob?.def == JobDefOf.DoBill && p.CurJob.bill == medicalBill);
    }

    /// <summary>
    /// Copied from HealthUtility.PartRemovalIntent(), plus return the target part's hediff
    /// </summary>
    private static BodyPartRemovalIntent PartRemovalIntent(Pawn pawn, BodyPartRecord part, out Hediff badHediff)
    {
        badHediff = pawn.health.hediffSet.hediffs.FirstOrDefault(d => d.Visible && d.Part == part && d.def.isBad && d.def != HediffDefOf.SurgicalCut);
        if (badHediff != null)
            return BodyPartRemovalIntent.Amputate;
        return BodyPartRemovalIntent.Harvest;
    }
}
