using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public class CasketAcceptedEvent(Pawn pawn, Building_Casket casket) : GameEventBase
{
    public Pawn Pawn { get; } = pawn;
    public Building_Casket Casket { get; } = casket;
}


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