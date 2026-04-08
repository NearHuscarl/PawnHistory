using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record CasketAcceptedEvent(Pawn Pawn, Building_Casket Casket) : GameEventBase;


[HarmonyPatch(typeof(Building_Casket), nameof(Building_Casket.TryAcceptThing))]
internal class Building_Casket_TryAcceptThing_Patch
{
    static void Postfix(Building_Casket __instance, bool __result, Thing thing)
    {
        if (!__result) return;

        if (thing is not Pawn pawn) return;

        GameEventBus.Publish(new CasketAcceptedEvent(pawn, __instance));
    }
}