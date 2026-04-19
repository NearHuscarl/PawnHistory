using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record ReceiveLetterEvent(TaggedString Label, TaggedString Text, Faction Faction, IEnumerable<Pawn> Pawns) : GameEventBase;

[HarmonyPatch(typeof(LetterStack), nameof(LetterStack.ReceiveLetter), typeof(TaggedString), typeof(TaggedString), typeof(LetterDef), typeof(LookTargets), typeof(Faction), typeof(Quest), typeof(List<ThingDef>), typeof(string), typeof(int), typeof(bool))]
internal static class LetterStack_ReceiveLetter_Patch_2
{
    public static void Prefix(TaggedString label, TaggedString text, LookTargets lookTargets, Faction relatedFaction)
    {
        var pawns = lookTargets.GetPawns().ToList();
        GameEventBus.Publish(new ReceiveLetterEvent(label, text, relatedFaction, pawns));
    }
}
