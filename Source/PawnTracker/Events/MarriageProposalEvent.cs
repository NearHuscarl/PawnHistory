using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum MarriageProposalOutcome
{
    Accepted,
    Rejected,
    RejectedAndBrokeUp,
}

public record MarriageProposalEvent(Pawn Initiator, Pawn Recipient, string LogEntryText, MarriageProposalOutcome Outcome) : GameEventBase;

[HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
internal class PlayLog_Add_Patch_6
{
    private static void Postfix(LogEntry entry)
    {
        if (entry is not PlayLogEntry_Interaction interactionEntry)
            return;
        
        var interactionDef = Accessor.PlayLogEntry_Interaction.InteractionDef(interactionEntry);
        if (interactionDef != InteractionDefOf.MarriageProposal)
            return;

        var initiator = Accessor.PlayLogEntry_Interaction.Initiator(interactionEntry);
        var recipient = Accessor.PlayLogEntry_Interaction.Recipient(interactionEntry);
        if (initiator == null || recipient == null)
            return;
        var extraSentencePacks = Accessor.PlayLogEntry_Interaction.ExtraSentencePacks(interactionEntry).ToHashSet();
        var outcome = MarriageProposalOutcome.RejectedAndBrokeUp;
        
        if (extraSentencePacks.Contains(RulePackDefOf.Sentence_MarriageProposalAccepted))
            outcome =  MarriageProposalOutcome.Accepted;
        else if (extraSentencePacks.Contains(RulePackDefOf.Sentence_MarriageProposalRejectedBrokeUp)) // Note: RejectedAndBrokeUp contains both Sentence_MarriageProposalRejected & Sentence_MarriageProposalRejectedBrokeUp
            outcome =  MarriageProposalOutcome.RejectedAndBrokeUp;
        else if (extraSentencePacks.Contains(RulePackDefOf.Sentence_MarriageProposalRejected))
            outcome =  MarriageProposalOutcome.Rejected;
        
        var logEntryText = interactionEntry.ToGameStringFromPOV(recipient);
        
        GameEventBus.Publish(new MarriageProposalEvent(initiator, recipient, logEntryText, outcome));
    }
}
