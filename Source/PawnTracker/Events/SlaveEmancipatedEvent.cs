using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum SlaveEmancipatedCause
{
    SocialInteraction,
    BabyToChild,
}

public record SlaveEmancipatedEvent(Pawn Slave, Pawn Warden, SlaveEmancipatedCause Cause) : GameEventBase;

[HarmonyPatch(typeof(GenGuest), nameof(GenGuest.EmancipateSlave))]
internal static class GenGuest_EmancipateSlave_Patch
{
    private static void Postfix(Pawn warden, Pawn slave)
    {
        GameEventBus.Publish(new SlaveEmancipatedEvent(slave, warden, SlaveEmancipatedCause.SocialInteraction));
    }
}

[HarmonyPatch(typeof(ChoiceLetter_BabyToChild), "get_Choices")]
internal static class ChoiceLetter_BabyToChild_Choices_Patch_2
{
    private static void Postfix(ChoiceLetter_BabyToChild __instance, ref IEnumerable<DiaOption> __result)
    {
        var optionText = "Emancipate".Translate().CapitalizeFirst();

        __result = __result.Select(option =>
        {
            if (Accessor.DiaOption.Text(option) != optionText)
                return option;
            
            var originalAction = option.action;
            option.action = () =>
            {
                originalAction();

                var pawn = Accessor.ChoiceLetter_BabyToChild.Pawn(__instance);
                GameEventBus.Publish(new SlaveEmancipatedEvent(pawn, null, SlaveEmancipatedCause.BabyToChild));
            };

            return option;
        });
    }
}
