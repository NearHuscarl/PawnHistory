using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Harmony;

[HarmonyPatch(
    typeof(PrisonBreakUtility),
    nameof(PrisonBreakUtility.StartPrisonBreak),
    [typeof(Pawn), typeof(string), typeof(string), typeof(LetterDef), typeof(List<Pawn>)],
    [ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out]
)]
public static class PrisonBreakUtility_StartPrisonBreak_Patch
{
    public static void Postfix(Pawn initiator, string letterText, string letterLabel, LetterDef letterDef, List<Pawn> escapingPrisoners)
    {
        GameEventListener.Publish(new PrisonBreakStartedEvent(initiator, escapingPrisoners));
    }
}