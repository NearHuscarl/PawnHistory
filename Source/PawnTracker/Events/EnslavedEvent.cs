using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record EnslavedEvent(Pawn Slave, Pawn Enslaver, string LogEntryText = null) : GameEventBase;

// Call order:
// Pawn_InteractionsTracker.TryInteractWith()
// - InteractionWorker_EnslaveAttempt.Interacted()
//  - GenGuest.TryEnslavePrisoner() --> update GuestStatus
// - Find.PlayLog.Add(entry) --> here

[HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
internal class PlayLog_Add_Patch_8
{
    private static void Postfix(LogEntry entry)
    {
        if (entry is not PlayLogEntry_Interaction interactionEntry)
            return;

        if (Accessor.PlayLogEntry_Interaction.InteractionDef(interactionEntry) != InteractionDefOf.EnslaveAttempt)
            return;

        var initiator = Accessor.PlayLogEntry_Interaction.Initiator(interactionEntry);
        var recipient = Accessor.PlayLogEntry_Interaction.Recipient(interactionEntry);
        if (initiator == null || recipient == null)
            return;

        if (!recipient.IsSlaveOfColony)
            return;

        var logEntryText = interactionEntry.ToGameStringFromPOV(recipient);
        GameEventBus.Publish(new EnslavedEvent(recipient, initiator, logEntryText));
    }
}
