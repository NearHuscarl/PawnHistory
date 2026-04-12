using System;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class BreakupRecorder : RecorderBase<BreakupEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<BreakupEvent>(CreateRecord);
    }

    public override void CreateRecord(BreakupEvent e)
    {
        var recordDef = HistoryRecordDefOf.Breakup;
        var desc = recordDef.Description(e.Initiator, "Dumper")
            .IncludePawnGrammar()
            .AddRule("Rejected", e.Recipient)
            .AddRule("Thought", e.Reason.Colorize(NeedsCardUtility.MoodColorNegative))
            .AddConstant("hasReason", e.Reason != null)
            .Resolve();
        
        if (ShouldRecord(e.Initiator))
            AddRecord(recordDef, e.Initiator, desc, [e.Recipient]);
        
        if (ShouldRecord(e.Recipient))
            AddRecord(recordDef, e.Recipient, desc, [e.Initiator]);
    }

    public void Test(TestScenario scenario)
    {
        var breakupInteraction = DefDatabase<InteractionDef>.GetNamed("Breakup");
        var recipient = scenario.Pawn()
            .Colonist()
            .CreateSingle();
        var initiator = scenario.Pawn()
            .Colonist()
            .SetRelation(recipient, PawnRelationDefOf.Lover)
            .WithPosition(recipient.Position) 
            .Do((p, i, pawns) => p.interactions.TryInteractWith(recipient, breakupInteraction))
            .CreateSingle();

        Expect.That(initiator).ToHaveHistoryRecord("[Dumper] broke up with [Rejected]. [Tale]", HistoryRecordDefOf.Breakup);
        Expect.That(recipient).ToHaveHistoryRecord("[Dumper] broke up with [Rejected]. [Tale]", HistoryRecordDefOf.Breakup);
    }

    public void TestWithReason(TestScenario scenario)
    {
        var breakupInteraction = DefDatabase<InteractionDef>.GetNamed("Breakup");
        var recipient = scenario.Pawn()
            .Colonist()
            .CreateSingle();
        var initiator = scenario.Pawn()
            .Colonist()
            .SetRelation(recipient, PawnRelationDefOf.Lover)
            .WithPosition(recipient.Position) 
            .Do((p, i, pawns) => p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.CheatedOnMe, recipient))
            .Do((p, i, pawns) => p.interactions.TryInteractWith(recipient, breakupInteraction))
            .CreateSingle();

        Expect.That(initiator).ToHaveHistoryRecord("[Dumper] broke up with [Rejected]. [Tale]. The final straw was: Cheated on me.", HistoryRecordDefOf.Breakup);
        Expect.That(recipient).ToHaveHistoryRecord("[Dumper] broke up with [Rejected]. [Tale]. The final straw was: Cheated on me.", HistoryRecordDefOf.Breakup);
    }
}
