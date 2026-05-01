using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record BabyAdoptedEvent(Pawn Baby, Faction FormerFaction) : GameEventBase;
public record BabyAdoptedState(Faction FactionBefore);

[HarmonyPatch(typeof(Designator_Adopt), nameof(Designator_Adopt.DesignateThing))]
internal static class Designator_Adopt_DesignateThing_Patch
{
    private static void Prefix(Thing t, out BabyAdoptedState __state)
    {
        __state = new BabyAdoptedState(t.Faction);
    }
    private static void Postfix(Thing t, BabyAdoptedState __state)
    {
        GameEventBus.Publish(new BabyAdoptedEvent(t as Pawn, __state.FactionBefore));
    }
}
