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

    static void Postfix(Pawn_HealthTracker __instance, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageResult result)
    {
        if (hediff.def == HediffDefOf.MissingBodyPart && part != null)
        {
            var pawn = PawnRef(__instance);
            if (!PawnTracker.ShouldTrack(pawn)) return;

            // missing vital body part will make a pawn die, this is handled by in-game combat log. TODO: how to handle harvesting
            if (HediffUtility.IsPartVital(part, pawn))
                return;

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
    }
}
