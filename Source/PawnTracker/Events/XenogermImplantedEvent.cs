using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record XenogermImplantedEvent(Pawn Pawn, string XenotypeName, List<GeneDef> Genes, string OldXenotypeName, Pawn Donor) : GameEventBase;

internal static class XenogermImplantedContext
{
    public static string GetOldXenotypeName(Pawn pawn)
    {
        return pawn.genes?.UniqueXenotype == true ? null : pawn.genes?.XenotypeLabel;
    }

    public static void Publish(Pawn pawn, List<GeneDef> genes, string oldXenotypeName, Pawn donor = null)
    {
        if (pawn?.genes == null || genes.Count == 0)
            return;

        var xenotypeName = pawn.genes.XenotypeLabel;
        GameEventBus.Publish(new XenogermImplantedEvent(pawn, xenotypeName, genes, oldXenotypeName, donor));
    }
}

[HarmonyPatch(typeof(GeneUtility), nameof(GeneUtility.ImplantXenogermItem))]
internal static class GeneUtility_ImplantXenogermItem_Patch
{
    private static void Prefix(Pawn pawn, out string __state)
    {
        __state = XenogermImplantedContext.GetOldXenotypeName(pawn);
    }

    private static void Postfix(Pawn pawn, Xenogerm xenogerm, string __state)
    {
        if (pawn.genes == null || xenogerm?.GeneSet == null)
            return;

        var genes = xenogerm.GeneSet.GenesListForReading.ToList();
        XenogermImplantedContext.Publish(pawn, genes, __state);
    }
}

[HarmonyPatch(typeof(GeneUtility), nameof(GeneUtility.ReimplantXenogerm))]
internal static class GeneUtility_ReimplantXenogerm_Patch
{
    private static void Prefix(Pawn recipient, out string __state)
    {
        __state = XenogermImplantedContext.GetOldXenotypeName(recipient);
    }

    private static void Postfix(Pawn caster, Pawn recipient, string __state)
    {
        if (recipient.genes == null)
            return;

        var genes = recipient.genes.Xenogenes.Select(g => g.def).ToList();
        XenogermImplantedContext.Publish(recipient, genes, __state, caster);
    }
}
