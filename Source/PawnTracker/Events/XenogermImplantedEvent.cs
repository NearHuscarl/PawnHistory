using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record XenogermImplantedEvent(
    Pawn Pawn,
    string XenotypeName,
    List<GeneDef> Genes,
    string OldXenotypeName) : GameEventBase;

internal readonly record struct XenogermImplantedState(string OldXenotypeName);

[HarmonyPatch(typeof(GeneUtility), nameof(GeneUtility.ImplantXenogermItem))]
internal static class GeneUtility_ImplantXenogermItem_Patch
{
    private static void Prefix(Pawn pawn, out XenogermImplantedState __state)
    {
        __state = default;

        if (pawn.genes?.UniqueXenotype == true)
            return;

        __state = new XenogermImplantedState(pawn.genes!.XenotypeLabel);
    }

    private static void Postfix(Pawn pawn, Xenogerm xenogerm, XenogermImplantedState __state)
    {
        if (pawn?.genes == null || xenogerm?.GeneSet == null)
            return;

        var genes = xenogerm.GeneSet.GenesListForReading.ToList();
        if (genes.Count == 0)
            return;

        var xenotypeName = pawn.genes.XenotypeLabel;

        GameEventBus.Publish(new XenogermImplantedEvent(
            pawn,
            xenotypeName,
            genes,
            __state.OldXenotypeName));
    }
}
