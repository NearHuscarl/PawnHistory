using System;
using HarmonyLib;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

[HarmonyPatch(typeof(QuestPart_RefugeeInteractions), "ChooseRandomInteraction")]
public class RefugeeAlwaysAssaultOnViolation
{
    private static readonly Type ResponseType = AccessTools.Inner(typeof(QuestPart_RefugeeInteractions), "InteractionResponseType");
    private static void Postfix(ref object __result)
    {
        if (TestManager.Scenario.RefugeeAlwaysAssaultOnViolation)
            __result = Enum.Parse(ResponseType, "AssaultColony");
    }
}