using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record AncientDangerWarningEvent(Pawn Pawn) : GameEventBase;

[HarmonyPatch(typeof(LetterStack), nameof(LetterStack.ReceiveLetter), typeof(TaggedString), typeof(TaggedString), typeof(LetterDef), typeof(LookTargets), typeof(Faction), typeof(Quest), typeof(List<ThingDef>), typeof(string), typeof(int), typeof(bool))]
internal static class LetterStack_ReceiveLetter_AncientDangerWarning_Patch
{
    private static readonly string LetterLabel = "LetterLabelAncientShrineWarning".Translate();

    public static void Prefix(TaggedString label, LookTargets lookTargets)
    {
        if (label.Resolve() != LetterLabel)
            return;

        var pawn = lookTargets.GetPawns().FirstOrDefault();
        if (pawn == null)
            return;

        GameEventBus.Publish(new AncientDangerWarningEvent(pawn));
    }
}
