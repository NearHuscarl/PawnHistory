using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record PrisonerRecruitedEvent(Pawn Prisoner, Pawn Recruiter, string LogEntryText = null) : GameEventBase;

// Call order:
// Pawn_InteractionsTracker.TryInteractWith() prefix
// InteractionWorker_RecruitAttempt.Interacted()
// - InteractionWorker_RecruitAttempt.DoRecruit() --> update Faction
// Find.PlayLog.Add(entry) --> here
// Pawn_InteractionsTracker.TryInteractWith() postfix

[HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
internal class PlayLog_Add_Patch_3
{
    internal static void Postfix(LogEntry entry)
    {
        if (entry is not PlayLogEntry_Interaction interactionEntry)
            return;
        
        var interactionDef = Accessor.PlayLogEntry_Interaction.InteractionDef(interactionEntry);
        if (interactionDef != InteractionDefOf.RecruitAttempt)
            return;

        var initiator = Accessor.PlayLogEntry_Interaction.Initiator(interactionEntry);
        var recipient = Accessor.PlayLogEntry_Interaction.Recipient(interactionEntry);
        if (initiator == null || recipient == null)
            return;

        if (initiator.Faction != recipient.Faction)
            return;
        
        var logEntryText = interactionEntry.ToGameStringFromPOV(recipient);
        GameEventBus.Publish(new PrisonerRecruitedEvent(recipient, initiator, logEntryText));
    }
}
