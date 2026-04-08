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
