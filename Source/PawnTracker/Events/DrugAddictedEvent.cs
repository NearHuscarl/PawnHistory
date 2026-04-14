using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record DrugAddictedEvent(Pawn Pawn, Hediff Hediff, ChemicalDef Chemical) : GameEventBase;

internal static class DrugAddictedContext
{
    public static List<Hediff> HediffSnapshot;
    public static List<Hediff> GetHediffSnapshot(Pawn pawn) => pawn.health.hediffSet.hediffs.ToList();
}

[HarmonyPatch(typeof(CompDrug), nameof(CompDrug.PrePostIngested))]
internal static class CompDrug_PrePostIngested_Patch
{
    private static void Prefix(Pawn ingester)
    {
        DrugAddictedContext.HediffSnapshot = DrugAddictedContext.GetHediffSnapshot(ingester);
    }

    private static void Postfix(CompDrug __instance, Pawn ingester)
    {
        if (DrugAddictedContext.HediffSnapshot == null)
            return;

        var hediff = DrugAddictedContext.GetHediffSnapshot(ingester).Except(DrugAddictedContext.HediffSnapshot).FirstOrDefault();
        var chemical = __instance.Props.chemical;
        
        if (hediff?.def != chemical.addictionHediff)
            return;
        
        GameEventBus.Publish(new DrugAddictedEvent(ingester, hediff, chemical));
    }

    private static void Finalizer() => DrugAddictedContext.HediffSnapshot = null;
}
