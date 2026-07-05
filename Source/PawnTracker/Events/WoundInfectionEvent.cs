using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record WoundInfectionEvent(Hediff Infection, Hediff SourceWound) : GameEventBase;

[HarmonyPatch(typeof(HediffComp_Infecter), "CheckMakeInfection")]
internal static class HediffComp_Infecter_CheckMakeInfection_Patch
{
    private static void Postfix(HediffComp_Infecter __instance)
    {
        if (Accessor.HediffComp_Infecter.TicksUntilInfect(__instance) != Accessor.HediffComp_Infecter.AlreadyMadeInfectionValue)
            return;

        var sourceWound = __instance.parent;
        var pawn = sourceWound.pawn;
        var infection = pawn.health.hediffSet.hediffs.LastOrDefault(h => h.Part == sourceWound.Part && (h.def == HediffDefOf.WoundInfection || h.def == HediffDefOf.ScariaInfection));

        GameEventBus.Publish(new WoundInfectionEvent(infection, sourceWound));
    }
}
