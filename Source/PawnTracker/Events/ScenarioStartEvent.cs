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
    static void Postfix(ScenPart_PlayerPawnsArriveMethod __instance)
    {
        var pawns = Find.GameInitData.startingAndOptionalPawns;
        var arriveMethod = Accessor.ScenPart_PlayerPawnsArriveMethod.Method(__instance);
        GameEventBus.Publish(new ScenarioStartEvent(pawns, arriveMethod));
    }
}
