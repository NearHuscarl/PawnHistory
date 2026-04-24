using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record MarriedEvent(Pawn FirstPawn, Pawn SecondPawn) : GameEventBase;

[HarmonyPatch(typeof(MarriageCeremonyUtility), nameof(MarriageCeremonyUtility.Married))]
internal static class MarriageCeremonyUtility_Married_Patch
{
    private static void Postfix(Pawn firstPawn, Pawn secondPawn)
    {
        GameEventBus.Publish(new MarriedEvent(firstPawn, secondPawn));
    }
}
