using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public class ScenarioStartEvent(List<Pawn> startingPawns, PlayerPawnsArriveMethod arriveMethod) : GameEventBase
{
    public List<Pawn> StartingPawns { get; } = startingPawns;
    public PlayerPawnsArriveMethod ArriveMethod { get; } = arriveMethod;
}

[HarmonyPatch(typeof(ScenPart_PlayerPawnsArriveMethod), nameof(ScenPart_PlayerPawnsArriveMethod.GenerateIntoMap))]
internal class ScenPart_PlayerPawnsArriveMethod_GenerateIntoMap_Patch
{
    static readonly AccessTools.FieldRef<ScenPart_PlayerPawnsArriveMethod, PlayerPawnsArriveMethod> MethodRef =
        AccessTools.FieldRefAccess<ScenPart_PlayerPawnsArriveMethod, PlayerPawnsArriveMethod>("method");

    static void Postfix(ScenPart_PlayerPawnsArriveMethod __instance)
    {
        var pawns = Find.GameInitData.startingAndOptionalPawns;
        GameEventBus.Publish(new ScenarioStartEvent(pawns, MethodRef(__instance)));
    }
}