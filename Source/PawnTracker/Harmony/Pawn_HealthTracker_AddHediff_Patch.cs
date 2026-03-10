using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;
using static Verse.DamageWorker;

namespace PawnHistory.Source.PawnTracker;

[HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), [typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageResult)])]
internal class Pawn_HealthTracker_AddHediff_Patch
{
    static readonly AccessTools.FieldRef<Pawn_HealthTracker, Pawn> PawnRef = AccessTools.FieldRefAccess<Pawn_HealthTracker, Pawn>("pawn");

    static void Prefix(Pawn_HealthTracker __instance, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageResult result)
    {
        part ??= hediff.Part;
        var pawn = PawnRef(__instance);
        if (part == null) return;

        if (!PawnTracker.ShouldTrack(pawn))
            return;

        if (hediff.def == HediffDefOf.MissingBodyPart)
        {
            if (dinfo?.Def == DamageDefOf.SurgicalCut)
                HandleRemovePartEvent(pawn, hediff, part);
        }
    }
    static void Postfix(Pawn_HealthTracker __instance, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageResult result)
    {
        part ??= hediff.Part;
        var pawn = PawnRef(__instance);
        if (part == null) return;

        if (!PawnTracker.ShouldTrack(pawn))
            return;

        if (hediff.def == HediffDefOf.MissingBodyPart)
        {
            // missing vital body part will make a pawn die, this is handled by in-game combat log instead.
            if (HediffUtility.IsPartVital(part, pawn))
                return;

            if (dinfo?.Def != DamageDefOf.SurgicalCut)
                HandleDestroyPartEvent(pawn, hediff, part, dinfo);
        }
        else if (hediff.IsPermanent() && dinfo.HasValue /* from combat rather than old wound */)
        {
            HandleScarredPartEvent(pawn, hediff, part, dinfo);
        }
    }

    // Must be called in postfix because hediff.label does not exist in prefix.
    private static void HandleDestroyPartEvent(Pawn pawn, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo)
    {
        var instigator = dinfo?.Instigator as Pawn;
        var weapon = dinfo?.Weapon?.label ?? dinfo?.Def.label;
        var eventDef = PawnEventDefOf.BodyPartLost;
        var request = new GrammarRequest();

        request.Includes.Add(eventDef.rulePackDef);
        request.Rules.Add(new Rule_String("PAWN", pawn.NameShortColored.Resolve()));
        request.Rules.Add(new Rule_String("PART", part.Label.Colorize(hediff.LabelColor)));
        request.Rules.Add(new Rule_String("HEDIFF", hediff.LabelBase.ToLower().Colorize(hediff.LabelColor))); // <destroyedLabel>

        if (dinfo?.Instigator is Pawn)
        {
            request.Rules.Add(new Rule_String("INSTIGATOR", instigator.NameShortColored.Resolve()));
            request.Constants.Add("hasInstigator", "true");
        }

        if (weapon != null)
        {
            request.Rules.Add(new Rule_String("WEAPON", weapon));
            request.Constants.Add("hasWeapon", "true");
        }

        var resolvedDesc = GrammarResolver.Resolve("bodyPartLost", request);
        GameEventListener.Publish(new GameEvent(pawn, eventDef, resolvedDesc) { relatedPawns = [instigator] });
    }

    private static void HandleScarredPartEvent(Pawn pawn, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo)
    {
        var instigator = dinfo?.Instigator as Pawn;
        var weapon = dinfo?.Weapon?.label ?? dinfo?.Def.label;
        var eventDef = PawnEventDefOf.BodyPartPermanentlyDamaged;
        var request = new GrammarRequest();

        request.Includes.Add(eventDef.rulePackDef);
        request.Rules.AddRange(GrammarUtility.RulesForPawn("PAWN", pawn));
        request.Rules.Add(new Rule_String("PART", part.Label.Colorize(hediff.LabelColor)));
        request.Rules.Add(new Rule_String("HEDIFF", hediff.LabelBase.ToLower().Colorize(hediff.LabelColor))); // <permanentLabel>

        if (dinfo?.Instigator is Pawn)
        {
            request.Rules.Add(new Rule_String("INSTIGATOR", instigator.NameShortColored.Resolve()));
            request.Constants.Add("hasInstigator", "true");
        }

        if (weapon != null)
        {
            request.Rules.Add(new Rule_String("WEAPON", weapon));
            request.Constants.Add("hasWeapon", "true");
        }

        var resolvedDesc = GrammarResolver.Resolve("bodyPartPermanentlyDamaged", request);
        GameEventListener.Publish(new GameEvent(pawn, eventDef, resolvedDesc) { relatedPawns = [instigator] });
    }

    // Must be called in prefix to retrieve the bad hediff before amptuation.
    private static void HandleRemovePartEvent(Pawn pawn, Hediff hediff, BodyPartRecord part)
    {
        var eventDef = PawnEventDefOf.BodyPartLost;
        var request = new GrammarRequest();
        var doctor = GetOperatingDoctor(pawn);
        var removeIntent = PartRemovalIntent(pawn, part, out Hediff badHediff);

        request.Includes.Add(eventDef.rulePackDef);
        request.Rules.Add(new Rule_String("PAWN", pawn.NameShortColored.Resolve()));
        request.Rules.Add(new Rule_String("PART", part.Label.Colorize(hediff.LabelColor)));
        request.Rules.Add(new Rule_String("DOCTOR", doctor.NameShortColored.Resolve()));
        request.Constants.Add("intent", removeIntent.ToString());

        if (badHediff != null)
            request.Rules.Add(new Rule_String("HEDIFF", badHediff.LabelBase.ToLower().Colorize(badHediff.LabelColor)));

        var resolvedDesc = GrammarResolver.Resolve("bodyPartLostSurgery", request);

        GameEventListener.Publish(new GameEvent(pawn, eventDef, resolvedDesc) { relatedPawns = [doctor] });
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
