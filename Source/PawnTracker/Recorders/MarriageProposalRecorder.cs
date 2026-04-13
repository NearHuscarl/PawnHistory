using System;
using System.Linq;
using PawnHistory.Source.DebugTools;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class MarriageProposalRecorder : RecorderBase<MarriageProposalEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<MarriageProposalEvent>(CreateRecord);
    }

    public override void CreateRecord(MarriageProposalEvent e)
    {
        var recordDef = HistoryRecordDefOf.MarriageProposal;
        // remove Sentence_MarriageProposalAccepted, Sentence_MarriageProposalRejected, Sentence_MarriageProposalRejectedBrokeUp
        var marriageProposalText = e.LogEntryText.Split('.').Select(p => p.Trim()).FirstOrDefault(p => !p.NullOrEmpty());
        var desc = recordDef.Description(e.Recipient)
            .AddRule("InteractionLog", marriageProposalText)
            .AddConstant("outcome", e.Outcome)
            .Resolve();
        
        if (ShouldRecord(e.Initiator))
            AddRecord(recordDef, e.Initiator, desc, [e.Recipient]);
        if (ShouldRecord(e.Recipient))
            AddRecord(recordDef, e.Recipient, desc, [e.Initiator]);
    }

    public Action TestAccepted(TestScenario scenario)
    {
        NearDebugSettings.ForceMarriageProposalAccepted = true;
        
        var recipient = scenario.Pawn()
            .Colonist()
            .CreateSingle();
        var initiator = scenario.Pawn()
            .Colonist()
            .WithPosition(recipient.Position)
            .Do(p => p.interactions.TryInteractWith(recipient, InteractionDefOf.MarriageProposal))
            .CreateSingle();

        Expect.That(initiator).ToHaveHistoryRecord("[InteractionLog]. [PAWN] agreed and the two became engaged.", HistoryRecordDefOf.MarriageProposal);
        Expect.That(recipient).ToHaveHistoryRecord("[InteractionLog]. [PAWN] agreed and the two became engaged.", HistoryRecordDefOf.MarriageProposal);
        
        return () => NearDebugSettings.ForceMarriageProposalAccepted = false;
    }

    public Action TestRejected(TestScenario scenario)
    {
        NearDebugSettings.ForceMarriageProposalRejected = true;
        
        var recipient = scenario.Pawn()
            .Colonist()
            .CreateSingle();
        var initiator = scenario.Pawn()
            .Colonist()
            .WithPosition(recipient.Position)
            .Do(p => p.interactions.TryInteractWith(recipient, InteractionDefOf.MarriageProposal))
            .CreateSingle();

        if (initiator.relations.DirectRelationExists(PawnRelationDefOf.ExLover, recipient))
        {
            Expect.That(initiator).ToHaveHistoryRecord("[InteractionLog]. [PAWN] rejected the proposal. The rejection was too much for the relationship, and the two broke up.", HistoryRecordDefOf.MarriageProposal);
            Expect.That(recipient).ToHaveHistoryRecord("[InteractionLog]. [PAWN] rejected the proposal. The rejection was too much for the relationship, and the two broke up.", HistoryRecordDefOf.MarriageProposal);
        }
        else
        {
            Expect.That(initiator).ToHaveHistoryRecord("[InteractionLog]. [PAWN] rejected the proposal.", HistoryRecordDefOf.MarriageProposal, exactMatch: true);
            Expect.That(recipient).ToHaveHistoryRecord("[InteractionLog]. [PAWN] rejected the proposal.", HistoryRecordDefOf.MarriageProposal, exactMatch: true);
        }
        
        return () => NearDebugSettings.ForceMarriageProposalRejected = false;
    }
}
