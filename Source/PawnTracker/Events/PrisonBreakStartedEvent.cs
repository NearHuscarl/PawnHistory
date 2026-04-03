using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public class PrisonBreakStartedEvent(Pawn initiator, List<Pawn> escapingPrisoners, PrisonBreakReason reason, string logEntryText = null) : GameEventBase
{
    public Pawn Initiator { get; } = initiator;
    public List<Pawn> EscapingPrisoners { get; } = escapingPrisoners ?? [];
    public PrisonBreakReason Reason { get; } = reason;
    public string LogEntryText { get; } = logEntryText;
}

public enum PrisonBreakReason
{
    None,
    Rebellion,
    JailBreaker,
}

public static class PrisonBreakStartedContext
{
    public static PrisonBreakReason Reason;
    public static List<Pawn> EscapingPrisoners;
    public static void Reset()
    {
        Reason = PrisonBreakReason.None;
        EscapingPrisoners = [];
    }
}

// Call order:
// Pawn_InteractionsTracker.TryInteractWith() prefix
// PrisonBreakUtility.StartPrisonBreak()
// PlayLog.Add()
// Pawn_InteractionsTracker.TryInteractWith() postfix

[HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractWith))]
public static class Pawn_InteractionsTracker_TryInteractWith_Patch
{
    static void Prefix(InteractionDef intDef)
    {
        if (intDef == InteractionDefOf.SparkJailbreak)
            PrisonBreakStartedContext.Reason = PrisonBreakReason.JailBreaker;
    }

    static void Finalizer()
    {
        PrisonBreakStartedContext.Reset();
    }
}

[HarmonyPatch(
    typeof(PrisonBreakUtility),
    nameof(PrisonBreakUtility.StartPrisonBreak),
    [typeof(Pawn), typeof(string), typeof(string), typeof(LetterDef), typeof(List<Pawn>)],
    [ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out]
)]
public static class PrisonBreakUtility_StartPrisonBreak_Patch
{
    public static void Postfix(Pawn initiator, List<Pawn> escapingPrisoners)
    {
        PrisonBreakStartedContext.EscapingPrisoners = escapingPrisoners;

        if (PrisonBreakStartedContext.Reason == PrisonBreakReason.JailBreaker)
            return;

        GameEventBus.Publish(new PrisonBreakStartedEvent(initiator, escapingPrisoners, PrisonBreakReason.Rebellion));
    }
}

[HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
internal class PlayLog_Add_Patch_2
{
    static void Postfix(LogEntry entry)
    {
        if (PrisonBreakStartedContext.Reason == PrisonBreakReason.JailBreaker)
        {
            if (entry is not PlayLogEntry_Interaction interactionEntry) return;

            var initiator = Accessor.PlayLogEntry_Interaction.Initiator(interactionEntry);
            if (initiator == null) return;

            if (Accessor.PlayLogEntry_Interaction.InteractionDef(interactionEntry) != InteractionDefOf.SparkJailbreak)
                return;

            var logText = entry.ToGameStringFromPOV(initiator);
            GameEventBus.Publish(new PrisonBreakStartedEvent(
                initiator,
                PrisonBreakStartedContext.EscapingPrisoners,
                PrisonBreakStartedContext.Reason,
                logText
            ));
        }
    }
}