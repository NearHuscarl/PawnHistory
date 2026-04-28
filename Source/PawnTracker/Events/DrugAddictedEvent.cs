using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record DrugAddictedEvent(Pawn Pawn, Hediff Hediff, ChemicalDef Chemical) : GameEventBase;

internal static class DrugAddictedContext
{
    public static List<Hediff> GetHediffSnapshot(Pawn pawn) => pawn.health.hediffSet.hediffs.ToList();
}

internal record DrugAddictedState(List<Hediff> HediffSnapshot);

[HarmonyPatch(typeof(CompDrug), nameof(CompDrug.PrePostIngested))]
internal static class CompDrug_PrePostIngested_Patch
{
    private static void Prefix(Pawn ingester, out DrugAddictedState __state)
    {
        var snapshot = DrugAddictedContext.GetHediffSnapshot(ingester);
        __state = new DrugAddictedState(snapshot);
    }

    private static void Postfix(CompDrug __instance, DrugAddictedState __state, Pawn ingester)
    {
        var hediff = DrugAddictedContext.GetHediffSnapshot(ingester).Except(__state.HediffSnapshot).FirstOrDefault();
        var chemical = __instance.Props.chemical;
        
        if (hediff?.def != chemical.addictionHediff)
            return;
        
        GameEventBus.Publish(new DrugAddictedEvent(ingester, hediff, chemical));
    }
}
