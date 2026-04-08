using HarmonyLib;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Events;

public record ScenarioPostGameStartEvent() : GameEventBase;

[HarmonyPatch(typeof(Scenario), nameof(Scenario.PostGameStart))]
internal class Scenario_PostGameStart_Patch
{
    static void Postfix()
    {
        GameEventBus.Publish(new ScenarioPostGameStartEvent());
    }
}
