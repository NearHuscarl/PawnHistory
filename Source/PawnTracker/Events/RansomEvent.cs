using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum RansomResult
{
    Accepted,
    Rejected,
    Postponed,
}

public record RansomEvent(Pawn Hostage, int SilverCount, Faction EnemyFaction, RansomResult Result) : GameEventBase;

[HarmonyPatch(typeof(ChoiceLetter_RansomDemand), "get_Choices")]
internal static class ChoiceLetter_RansomDemand_Choices_Patch
{
    private static void Postfix(ChoiceLetter_RansomDemand __instance, ref IEnumerable<DiaOption> __result)
    {
        var options = __result.ToList();
        if (options.Count == 1)
            return;

        foreach (var option in options)
        {
            var originalAction = option.action;
            var buttonText = Accessor.DiaOption.Text(option);
            var result = RansomResult.Postponed;
            
            if (buttonText == "RansomDemand_Accept".Translate())
                result = RansomResult.Accepted;
            else if (buttonText == "RejectLetter".Translate())
                result = RansomResult.Rejected;
            
            option.action = () =>
            {
                originalAction();
                var hostage = __instance.kidnapped;
                var faction = __instance.faction;

                GameEventBus.Publish(new RansomEvent(hostage, __instance.fee, faction, result));
            };
        }

        __result = options;
    }
}
