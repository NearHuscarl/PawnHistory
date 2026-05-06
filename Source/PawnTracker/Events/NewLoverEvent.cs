using System.Collections.Generic;
using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record NewLoverEvent(Pawn Initiator, Pawn Recipient, string LogEntryText, List<Pawn> InitiatorExes = null, List<Pawn> RecipientExes = null) : GameEventBase;

file static class NewLoverContext
{
    public static readonly Dictionary<Pawn, Pawn> CheatedLover = [];
}

// Call order:
// Pawn_InteractionsTracker.TryInteractWith() prefix
// - InteractionWorker_RomanceAttempt.Interacted()
//  - InteractionWorker_RomanceAttempt.TryAddCheaterThought()
// - Find.PlayLog.Add(entry) --> here
// Pawn_InteractionsTracker.TryInteractWith() postfix

[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "TryAddCheaterThought")]
internal class InteractionWorker_RomanceAttempt_TryAddCheaterThought_Patch
{
    private static void Postfix(Pawn pawn, Pawn cheater)
    {
        if (pawn.Dead)
            return;
        NewLoverContext.CheatedLover.Add(cheater, pawn);
    }
}

[HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
internal class PlayLog_Add_Patch_4
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
        
        if (!initiator.relations.DirectRelationExists(PawnRelationDefOf.Lover, recipient))
            return;

        var logEntryText = interactionEntry.ToGameStringFromPOV(recipient);
        
        // TODO: test affair condition in ideology
        var initiatorVictims = initiator.IsHavingAffairBasedOnIdeo() ? initiator.GetCurrentSpouses() : [];
        var recipientVictims = recipient.IsHavingAffairBasedOnIdeo() ? recipient.GetCurrentSpouses() : [];
        
        if (NewLoverContext.CheatedLover.TryGetValue(initiator, out var initiatorEx)) initiatorVictims.Add(initiatorEx);
        if (NewLoverContext.CheatedLover.TryGetValue(recipient, out var recipientEx)) recipientVictims.Add(recipientEx);
        
        GameEventBus.Publish(new NewLoverEvent(initiator, recipient, logEntryText, initiatorVictims,  recipientVictims));
        NewLoverContext.CheatedLover.Clear();
    }
}
