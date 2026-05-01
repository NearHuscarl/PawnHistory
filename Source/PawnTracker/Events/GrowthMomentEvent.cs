using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record GrowthMomentEvent(Pawn Pawn, List<SkillDef> SkillsWithNewPassion, Trait Trait) : GameEventBase;

internal record GrowthMomentState(Dictionary<SkillDef, Passion> SkillPassionsBefore, HashSet<Trait> TraitsBefore);

[HarmonyPatch(typeof(ChoiceLetter_GrowthMoment), nameof(ChoiceLetter_GrowthMoment.MakeChoices))]
internal static class ChoiceLetter_GrowthMoment_MakeChoices_Patch
{
    private static void Prefix(ChoiceLetter_GrowthMoment __instance, out GrowthMomentState __state)
    {
        // MakeChoices() can return early and ignore chosenPassions/chosenTrait in theory, so I compare the source just to be sure. 
        var skillPassions = __instance.pawn.skills.skills.ToDictionary(s => s.def, s => s.passion);
        var traits = __instance.pawn.story.traits.allTraits.ToHashSet();

        __state = new GrowthMomentState(skillPassions, traits);
    }
    
    private static void Postfix(ChoiceLetter_GrowthMoment __instance, GrowthMomentState __state)
    {
        if (!__instance.choiceMade)
            return;

        var skillsWithNewPassion = __instance.pawn.skills.skills
            .Where(s => s.passion != __state.SkillPassionsBefore[s.def])
            .Select(s => s.def)
            .ToList();
        var traits = __instance.pawn.story.traits.allTraits
            .Where(t => !__state.TraitsBefore.Contains(t))
            .ToList();
        var normalizedTrait = traits.Count == 0 ? null : traits[0];
        GameEventBus.Publish(new GrowthMomentEvent(__instance.pawn, skillsWithNewPassion, normalizedTrait));
    }
}
