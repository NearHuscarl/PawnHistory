using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Events;

public record SlaveRebellionEvent(Pawn Initiator, List<Pawn> EscapingSlaves, List<Pawn> EligibleSlaves, SlaveEscapeReason Reason, bool IsEscape, string LogEntryText = null) : GameEventBase;

public enum SlaveEscapeReason
{
    None,
    Rebellion,
    JailBreaker,
}

public static class SlaveRebellionContext
{
    public static SlaveEscapeReason Reason;
    public static List<Pawn> EscapingSlaves;
    public static List<Pawn> EligibleSlaves;
    
    public static bool IsEscape(List<Pawn> escapingSlaves)
    {
        return escapingSlaves.FirstOrDefault()?.GetLord()?.LordJob is LordJob_SlaveRebellion { IsAggressiveRebellion: false };
    }

    public static void Reset()
    {
        Reason = SlaveEscapeReason.None;
        EscapingSlaves = [];
        EligibleSlaves = [];
    }
}

// Call order:
// Pawn_InteractionsTracker.TryInteractWith() prefix
// - InteractionWorker_SparkSlaveRebellion.Interacted()
//  - SlaveRebellionUtility.StartSlaveRebellion()
// - PlayLog.Add()
// Pawn_InteractionsTracker.TryInteractWith() finalizer

[HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractWith))]
internal static class Pawn_InteractionsTracker_TryInteractWith_Patch_2
{
    private static void Prefix(InteractionDef intDef)
    {
        if (intDef == InteractionDefOf.SparkSlaveRebellion)
            SlaveRebellionContext.Reason = SlaveEscapeReason.JailBreaker;
    }

    private static void Finalizer() => SlaveRebellionContext.Reset();
}

[HarmonyPatch(
    typeof(SlaveRebellionUtility),
    nameof(SlaveRebellionUtility.StartSlaveRebellion),
    [typeof(Pawn), typeof(string), typeof(string), typeof(LetterDef), typeof(LookTargets), typeof(bool)],
    [ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Normal]
)]
internal static class SlaveRebellionUtility_StartSlaveRebellion_Patch
{
    private static void Prefix(Pawn initiator)
    {
        SlaveRebellionContext.EligibleSlaves = initiator.Map.mapPawns.SlavesOfColonySpawned
            .Where(SlaveRebellionUtility.CanParticipateInSlaveRebellion)
            .ToList();
    }

    private static void Postfix(Pawn initiator, LookTargets lookTargets, bool __result)
    {
        if (!__result)
            return;

        SlaveRebellionContext.EscapingSlaves = lookTargets.GetPawns().ToList();
        if (SlaveRebellionContext.Reason == SlaveEscapeReason.JailBreaker)
            return;

        GameEventBus.Publish(new SlaveRebellionEvent(
            initiator,
            SlaveRebellionContext.EscapingSlaves,
            SlaveRebellionContext.EligibleSlaves,
            SlaveEscapeReason.Rebellion,
            SlaveRebellionContext.IsEscape(SlaveRebellionContext.EscapingSlaves)
        ));
    }
}

[HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
internal class PlayLog_Add_Patch_9
{
    private static void Postfix(LogEntry entry)
    {
        if (SlaveRebellionContext.Reason != SlaveEscapeReason.JailBreaker)
            return;

        if (entry is not PlayLogEntry_Interaction interactionEntry)
            return;

        var initiator = Accessor.PlayLogEntry_Interaction.Initiator(interactionEntry);
        if (initiator == null)
            return;

        if (Accessor.PlayLogEntry_Interaction.InteractionDef(interactionEntry) != InteractionDefOf.SparkSlaveRebellion)
            return;

        var logText = entry.ToGameStringFromPOV(initiator);
        GameEventBus.Publish(new SlaveRebellionEvent(
            initiator,
            SlaveRebellionContext.EscapingSlaves,
            SlaveRebellionContext.EligibleSlaves,
            SlaveRebellionContext.Reason,
            SlaveRebellionContext.IsEscape(SlaveRebellionContext.EscapingSlaves),
            logText
        ));
    }
}
