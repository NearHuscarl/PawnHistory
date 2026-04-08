using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record ReceiveLetterEvent(TaggedString Label, IEnumerable<Pawn> Pawns) : GameEventBase;

[HarmonyPatch(typeof(LetterStack), nameof(LetterStack.ReceiveLetter), [typeof(TaggedString), typeof(TaggedString), typeof(LetterDef), typeof(LookTargets), typeof(Faction), typeof(Quest), typeof(List<ThingDef>), typeof(string), typeof(int), typeof(bool)])]
public static class LetterStack_ReceiveLetter_Patch_2
{
    public static void Prefix(TaggedString label, LookTargets lookTargets)
    {
        var pawns = lookTargets.GetPawns();
        if (pawns == null || !pawns.Any())
            return;

        GameEventBus.Publish(new ReceiveLetterEvent(label, pawns));
    }
}