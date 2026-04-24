using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record AnimalTamedEvent(Pawn Tamer, Pawn TamedPawn, string LogEntryText) : GameEventBase;

[HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
internal class PlayLog_Add_Patch_7
{
    internal static void Postfix(LogEntry entry)
    {
        if (entry is not PlayLogEntry_Interaction interactionEntry)
            return;
        
        var interactionDef = Accessor.PlayLogEntry_Interaction.InteractionDef(interactionEntry);
        if (interactionDef != InteractionDefOf.TameAttempt)
            return;

        var initiator = Accessor.PlayLogEntry_Interaction.Initiator(interactionEntry);
        var recipient = Accessor.PlayLogEntry_Interaction.Recipient(interactionEntry);
        if (initiator == null || recipient == null)
            return;

        if (initiator.Faction != recipient.Faction)
            return;

        var logEntryText = interactionEntry.ToGameStringFromPOV(recipient);
        GameEventBus.Publish(new AnimalTamedEvent(initiator, recipient, logEntryText));
    }
}
