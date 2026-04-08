using HarmonyLib;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Events;

public record JoinedLordEvent(IEnumerable<Pawn> Pawns, Lord Lord) : GameEventBase;

[HarmonyPatch(typeof(Lord), nameof(Lord.AddPawn))]
public static class Lord_AddPawn_Patch
{
    public static void Postfix(Lord __instance, Pawn p)
    {
        GameEventBus.Publish(new JoinedLordEvent([p], __instance));
    }
}

[HarmonyPatch(typeof(Lord), nameof(Lord.AddPawns))]
public static class Lord_AddPawns_Patch
{
    public static void Postfix(Lord __instance, IEnumerable<Pawn> pawns)
    {
        GameEventBus.Publish(new JoinedLordEvent(pawns, __instance));
    }
}
