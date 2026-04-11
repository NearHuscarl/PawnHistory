using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record BirthdayEvent(Pawn Pawn, List<HediffDef> AgingHediffs) : GameEventBase;

[HarmonyPatch(typeof(LetterStack), nameof(LetterStack.ReceiveLetter), typeof(TaggedString), typeof(TaggedString), typeof(LetterDef), typeof(LookTargets), typeof(Faction), typeof(Quest), typeof(List<ThingDef>), typeof(string), typeof(int), typeof(bool))]
internal static class LetterStack_ReceiveLetter_Patch
{
    private static readonly string LetterLabelBirthday = "LetterLabelBirthday".Translate();

    public static void Prefix(TaggedString label, LookTargets lookTargets)
    {
        if (label.Resolve() != LetterLabelBirthday)
            return;

        var pawn = lookTargets.GetPawns().FirstOrDefault();
        if (pawn == null)
            return;

        var hediffs = Accessor.Pawn_AgeTracker.tmpHediffsGained;
        if (hediffs.Count == 0)
            return;

        GameEventBus.Publish(new BirthdayEvent(pawn, hediffs));
    }
}
