using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;
using static Verse.DamageWorker;

namespace PawnHistory.Source.PawnTracker;

[HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), [typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult)])]
internal class Pawn_HealthTracker_AddHediff_Patch
{
    static readonly AccessTools.FieldRef<Pawn_HealthTracker, Pawn> PawnRef = AccessTools.FieldRefAccess<Pawn_HealthTracker, Pawn>("pawn");

    static void Prefix(Pawn_HealthTracker __instance, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageResult result)
    {
        if (hediff.def == HediffDefOf.MissingBodyPart && part != null)
        {
            var pawn = PawnRef(__instance);
            if (!PawnTracker.ShouldTrack(pawn)) return;

            if (dinfo.Value.Def == DamageDefOf.SurgicalCut)
                HandleSurgeryEvent(pawn, hediff, part, dinfo, result);
        }
    }
    static void Postfix(Pawn_HealthTracker __instance, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageResult result)
    {
        if (hediff.def == HediffDefOf.MissingBodyPart && part != null)
        {
            var pawn = PawnRef(__instance);
            if (!PawnTracker.ShouldTrack(pawn)) return;

            // missing vital body part will make a pawn die, this is handled by in-game combat log.
            if (HediffUtility.IsPartVital(part, pawn))
                return;

            if (dinfo.Value.Def != DamageDefOf.SurgicalCut)
                HandleCombatEvent(pawn, hediff, part, dinfo, result);
        }
    }

    // Must be called in postfix because hediff.label does not exist in prefix.
    private static void HandleCombatEvent(Pawn pawn, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageResult result)
    {
        var instigator = dinfo?.Instigator as Pawn;
        var weapon = dinfo?.Weapon?.label ?? dinfo?.Def.label;
        var eventDef = PawnEventDefOf.BodyPartLost;
        var request = new GrammarRequest();

        request.Includes.Add(eventDef.rulePackDef);
        request.Rules.Add(new Rule_String("PAWN", pawn.NameShortColored.Resolve()));
        request.Rules.Add(new Rule_String("PART", part.Label.Colorize(hediff.def.defaultLabelColor)));
        request.Rules.Add(new Rule_String("HEDIFF", hediff.LabelBase.ToLower().Colorize(hediff.def.defaultLabelColor)));

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

    private static void HandleSurgeryEvent(Pawn pawn, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageResult result)
    {
        var eventDef = PawnEventDefOf.BodyPartLost;
        var request = new GrammarRequest();
        var doctor = GetOperatingDoctor(pawn);
        Hediff badHediff;
        var removeIntent = PartRemovalIntent(pawn, part, out badHediff);

        request.Includes.Add(eventDef.rulePackDef);
        request.Rules.Add(new Rule_String("PAWN", pawn.NameShortColored.Resolve()));
        request.Rules.Add(new Rule_String("PART", part.Label.Colorize(hediff.def.defaultLabelColor)));
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
