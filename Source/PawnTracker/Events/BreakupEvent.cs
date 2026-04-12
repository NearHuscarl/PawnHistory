using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record BreakupEvent(Pawn Initiator, Pawn Recipient, string Reason) : GameEventBase;

internal static class BreakupContext
{
    public static string BreakupThought;
}

// Call order:
// Pawn_InteractionsTracker.TryInteractWith() prefix
// - InteractionWorker_Breakup.Interacted()
//  - InteractionWorker_Breakup.RandomBreakupReason()
// - Find.PlayLog.Add(entry)
// Pawn_InteractionsTracker.TryInteractWith() postfix

[HarmonyPatch(typeof(InteractionWorker_Breakup), nameof(InteractionWorker_Breakup.RandomBreakupReason))]
internal class InteractionWorker_Breakup_RandomBreakupReason_Patch
{
    private static void Postfix(Thought __result) => BreakupContext.BreakupThought = __result.LabelCap;
}

[HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
internal class PlayLog_Add_Patch_5
{
    private static readonly InteractionDef BreakupDef = DefDatabase<InteractionDef>.GetNamed("Breakup");
    
    private static void Postfix(LogEntry entry)
    {
        if (entry is not PlayLogEntry_Interaction interactionEntry)
            return;
        
        var interactionDef = Accessor.PlayLogEntry_Interaction.InteractionDef(interactionEntry);
        if (interactionDef != BreakupDef)
            return;

        var initiator = Accessor.PlayLogEntry_Interaction.Initiator(interactionEntry);
        var recipient = Accessor.PlayLogEntry_Interaction.Recipient(interactionEntry);
        if (initiator == null || recipient == null)
            return;
        
        GameEventBus.Publish(new BreakupEvent(initiator, recipient, BreakupContext.BreakupThought));
        BreakupContext.BreakupThought = null;
    }
}
