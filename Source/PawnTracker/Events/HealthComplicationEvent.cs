using HarmonyLib;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record HealthComplicationEvent(Pawn Pawn, HediffDef Condition, Hediff Cause) : GameEventBase;

[HarmonyPatch(typeof(HediffGiver), "SendLetter")]
public static class HediffGiver_SendLetter_Patch
{
    public static void Prefix(HediffGiver __instance, Pawn pawn, Hediff cause)
    {
        var condition = __instance.hediff;
        GameEventBus.Publish(new HealthComplicationEvent(pawn, condition, cause));
    }
}

[HarmonyPatch(typeof(HediffGiver_BrainInjury), nameof(HediffGiver_BrainInjury.OnHediffAdded))]
public static class HediffGiver_BrainInjury_OnHediffAdded_Patch
{
    private static void Postfix(HediffGiver_BrainInjury __instance, Pawn pawn, Hediff hediff, bool __result)
    {
        if (!__result)
            return;

        GameEventBus.Publish(new HealthComplicationEvent(pawn, __instance.hediff, hediff));
    }
}
