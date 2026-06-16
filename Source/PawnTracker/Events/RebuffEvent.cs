using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record RebuffEvent(Pawn Initiator, Pawn Recipient, string LogEntryText) : GameEventBase;

[HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
internal class PlayLog_Add_Patch_10
{
    private static void Postfix(LogEntry entry)
    {
        if (entry is not PlayLogEntry_Interaction interactionEntry)
            return;

        var interactionDef = Accessor.PlayLogEntry_Interaction.InteractionDef(interactionEntry);
        if (interactionDef != InteractionDefOf.RomanceAttempt)
            return;

        var initiator = Accessor.PlayLogEntry_Interaction.Initiator(interactionEntry);
        var recipient = Accessor.PlayLogEntry_Interaction.Recipient(interactionEntry);
        if (initiator == null || recipient == null)
            return;

        if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Lover, recipient))
            return;

        var logEntryText = interactionEntry.ToGameStringFromPOV(recipient);
        GameEventBus.Publish(new RebuffEvent(initiator, recipient, logEntryText));
    }
}
