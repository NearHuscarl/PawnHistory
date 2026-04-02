using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public class BirthdayEvent(Pawn pawn, List<HediffDef> agingHediffs) : GameEventBase
{
    public Pawn Pawn { get; } = pawn;
    public List<HediffDef> AgingHediffs { get; } = agingHediffs;
}

[HarmonyPatch(typeof(LetterStack), nameof(LetterStack.ReceiveLetter), [typeof(TaggedString), typeof(TaggedString), typeof(LetterDef), typeof(LookTargets), typeof(Faction), typeof(Quest), typeof(List<ThingDef>), typeof(string), typeof(int), typeof(bool)])]
public static class LetterStack_ReceiveLetter_Patch
{
    public static readonly List<HediffDef> tmpHediffsGainedRef = AccessTools.StaticFieldRefAccess<Pawn_AgeTracker, List<HediffDef>>("tmpHediffsGained");

    private static readonly string LetterLabelBirthday = "LetterLabelBirthday".Translate();

    public static void Prefix(TaggedString label, LookTargets lookTargets)
    {
        if (label.Resolve() != LetterLabelBirthday)
            return;

        var pawn = lookTargets.GetPawns().FirstOrDefault();
        if (pawn == null)
            return;

        var hediffs = tmpHediffsGainedRef;
        if (hediffs.Count == 0)
            return;

        GameEventBus.Publish(new BirthdayEvent(pawn, hediffs));
    }
}
