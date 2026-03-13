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
        GameEventListener.Subscribe<HediffPreAddEvent>(e =>
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

        GameEventListener.Subscribe<HediffPostAddEvent>(e =>
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
        var eventDef = PawnEventDefOf.BodyPartLost;
        var rules = new List<Rule>();
        var constants = new Dictionary<string, string>();
        var doctor = GetOperatingDoctor(pawn);
        var removeIntent = PartRemovalIntent(pawn, part, out Hediff badHediff);

        if (badHediff != null)
            rules.Add(new Rule_String("HEDIFF", badHediff.LabelBase.ToLower().Colorize(badHediff.LabelColor)));

        rules.Add(new Rule_String("DOCTOR", doctor.NameShortColored.Resolve()));
        rules.Add(new Rule_String("PART", part.Label.Colorize(hediff.LabelColor)));
        constants.Add("intent", removeIntent.ToString());

        var desc = eventDef.ResolveDescription(new DescriptionParams("bodyPartLostSurgery", pawn)
        {
            ExtraRules = rules,
            ExtraConstants = constants,
        });
        AddRecord(new HistoryRecord(eventDef, pawn, desc, [doctor]));
    }

    // Must be called in postfix because hediff.label does not exist in prefix.
    private void HandleDestroyPartEvent(Pawn pawn, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo)
    {
        var instigator = dinfo?.Instigator as Pawn;
        var weapon = dinfo?.Weapon?.label ?? dinfo?.Def.label;
        var eventDef = PawnEventDefOf.BodyPartLost;
        var rules = new List<Rule>();
        var constants = new Dictionary<string, string>();

        rules.Add(new Rule_String("PART", part.Label.Colorize(hediff.LabelColor)));
        rules.Add(new Rule_String("HEDIFF", hediff.LabelBase.ToLower().Colorize(hediff.LabelColor))); // <destroyedLabel>

        if (dinfo?.Instigator is Pawn)
        {
            rules.Add(new Rule_String("INSTIGATOR", instigator.NameShortColored.Resolve()));
            constants.Add("hasInstigator", "true");
        }

        if (weapon != null)
        {
            rules.Add(new Rule_String("WEAPON", weapon));
            constants.Add("hasWeapon", "true");
        }

        var desc = eventDef.ResolveDescription(new DescriptionParams("bodyPartLost", pawn)
        {
            ExtraRules = rules,
            ExtraConstants = constants,
        });
        System.Diagnostics.Debugger.Break();
        AddRecord(new HistoryRecord(eventDef, pawn, desc, [instigator]));
    }

    private void HandleScarredPartEvent(Pawn pawn, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo)
    {
        var instigator = dinfo?.Instigator as Pawn;
        var weapon = dinfo?.Weapon?.label ?? dinfo?.Def.label;
        var eventDef = PawnEventDefOf.BodyPartPermanentlyDamaged;
        var rules = new List<Rule>();
        var constants = new Dictionary<string, string>();

        rules.Add(new Rule_String("PART", part.Label.Colorize(hediff.LabelColor)));
        rules.Add(new Rule_String("HEDIFF", hediff.LabelBase.ToLower().Colorize(hediff.LabelColor))); // <permanentLabel>

        if (dinfo?.Instigator is Pawn)
        {
            rules.Add(new Rule_String("INSTIGATOR", instigator.NameShortColored.Resolve()));
            constants.Add("hasInstigator", "true");
        }
        if (weapon != null)
        {
            rules.Add(new Rule_String("WEAPON", weapon));
            constants.Add("hasWeapon", "true");
        }

        var desc = eventDef.ResolveDescription(new DescriptionParams("bodyPartPermanentlyDamaged", pawn)
        {
            AddRulesForPawn = true,
            ExtraRules = rules,
            ExtraConstants = constants,
        });
        AddRecord(new HistoryRecord(eventDef, pawn, desc, [instigator]));
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
