using HarmonyLib;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public class HealthComplicationEvent(Pawn pawn, HediffDef condition, Hediff cause) : GameEventBase
{
    public Pawn Pawn { get; } = pawn;
    public HediffDef Condition { get; } = condition;
    public Hediff Cause { get; } = cause;
}

[HarmonyPatch(typeof(HediffGiver), "SendLetter")]
public static class HediffGiver_SendLetter_Patch
{
    public static void Prefix(HediffGiver __instance, Pawn pawn, Hediff cause)
    {
        var condition = __instance.hediff;
        GameEventBus.Publish(new HealthComplicationEvent(pawn, condition, cause));
    }
}
