using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record PsylinkLevelGainedEvent(Pawn Pawn, int NewLevel, AbilityDef NewAbility) : GameEventBase;

public record PsylinkLevelGainedState(HashSet<AbilityDef> AbilitiesBefore);

[HarmonyPatch(typeof(Hediff_Psylink), nameof(Hediff_Psylink.TryGiveAbilityOfLevel))]
internal static class Hediff_Psylink_TryGiveAbilityOfLevel_Patch
{
    private static void Prefix(Hediff_Psylink __instance, out PsylinkLevelGainedState __state)
    {
        var abilities = __instance.pawn.abilities.abilities
            .Where(a => a.def.IsPsycast)
            .Select(a => a.def)
            .ToHashSet();

        __state = new PsylinkLevelGainedState(abilities);
    }

    private static void Postfix(Hediff_Psylink __instance, int abilityLevel, PsylinkLevelGainedState __state)
    {
        var pawn = __instance.pawn;
        var newAbility = pawn.abilities.abilities.FirstOrDefault(a => a.def.IsPsycast && a.def.level == abilityLevel && !__state.AbilitiesBefore.Contains(a.def));

        GameEventBus.Publish(new PsylinkLevelGainedEvent(pawn, abilityLevel, newAbility.def));
    }
}
