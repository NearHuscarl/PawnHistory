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
            .Position(recipient.Position)
            .Do(p => p.interactions.TryInteractWith(recipient, InteractionDefOf.MarriageProposal))
            .CreateSingle();

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.MarriageProposal,
            Description = "[InteractionLog]. [PAWN] agreed and the two became engaged.",
        };
        Expect.That(initiator).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [recipient] }));
        Expect.That(recipient).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [initiator] }));
        
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
            .Position(recipient.Position)
            .Do(p => p.interactions.TryInteractWith(recipient, InteractionDefOf.MarriageProposal))
            .CreateSingle();

        if (initiator.relations.DirectRelationExists(PawnRelationDefOf.ExLover, recipient))
        {
            var expected = new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.MarriageProposal,
                Description = "[InteractionLog]. [PAWN] rejected the proposal. The rejection was too much for the relationship, and the two broke up.",
            };
            Expect.That(initiator).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [recipient] }));
            Expect.That(recipient).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [initiator] }));
        }
        else
        {
            var expected = new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.MarriageProposal,
                Description = "[InteractionLog]. [PAWN] rejected the proposal.",
            };
            Expect.That(initiator).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [recipient] }), true);
            Expect.That(recipient).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [initiator] }), true);
        }
        
        return () => NearDebugSettings.ForceMarriageProposalRejected = false;
    }
}
