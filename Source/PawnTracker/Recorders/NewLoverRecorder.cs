using System;
using PawnHistory.Source.DebugTools;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class NewLoverRecorder : RecorderBase<NewLoverEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<NewLoverEvent>(CreateRecord);
    }

    public override void CreateRecord(NewLoverEvent e)
    {
        var recordDef = HistoryRecordDefOf.NewLover;
        // remove Sentence_RomanceAttemptAccepted
        var romanceAttemptText = e.LogEntryText.Split('.').Select(p => p.Trim()).FirstOrDefault(p => !p.NullOrEmpty());
        var desc = recordDef.Description(e.Initiator, "Initiator")
            .AddRule("Recipient", e.Recipient)
            .AddRule("InteractionLog", romanceAttemptText)
            .Resolve();

        if (ShouldRecord(e.Initiator))
        {
            AddRecord(recordDef, e.Initiator, desc, [e.Recipient]);
            foreach (var initiatorEx in e.InitiatorExes)
                CreateAffairRecord(e.Initiator, initiatorEx, e.Recipient);
        }

        if (ShouldRecord(e.Recipient))
        {
            AddRecord(recordDef, e.Recipient, desc, [e.Initiator]);
            foreach (var recipientEx in e.RecipientExes)
                CreateAffairRecord(e.Recipient, recipientEx, e.Initiator);
        }
    }

    private void CreateAffairRecord(Pawn cheater, Pawn victim, Pawn newLover)
    {
        var recordDef = HistoryRecordDefOf.NewAffair;
        var desc = recordDef.Description(cheater)
            .AddRule("Ex", victim)
            .AddRule("NewLover", newLover)
            .AddConstant("isMarried", cheater.relations.DirectRelationExists(PawnRelationDefOf.Spouse, victim))
            .Resolve();
        
        AddRecord(recordDef, cheater, desc, [victim, newLover]);
        AddRecord(recordDef, victim, desc, [cheater, newLover]);
    }

    public Action Test(TestScenario scenario)
    {
        NearDebugSettings.ForceRomanceSuccess = true;

        var recipient = scenario.Pawn()
            .Colonist()
            .CreateSingle();
        var initiator = scenario.Pawn()
            .Colonist()
            .Position(recipient.Position) 
            .Do((p, i, pawns) => p.interactions.TryInteractWith(recipient, InteractionDefOf.RomanceAttempt))
            .CreateSingle();
        
        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.NewLover,
            Description = "[InteractionLog]. [RomanceSuccessPrefix] [Initiator]'s lover.",
        };
        Expect.That(initiator).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [recipient] }));
        Expect.That(recipient).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [initiator] }));

        return () => NearDebugSettings.ForceRomanceSuccess = false;
    }

    public Action TestAffair(TestScenario scenario)
    {
        NearDebugSettings.ForceRomanceSuccess = true;

        var cuckold1 = scenario.Pawn().Colonist().CreateSingle();
        var cuckold2 = scenario.Pawn().Colonist().CreateSingle();
        var recipient = scenario.Pawn()
            .Colonist()
            .SetRelation(cuckold1, PawnRelationDefOf.Lover)
            .CreateSingle();
        var initiator = scenario.Pawn()
            .Colonist()
            .SetRelation(cuckold2, PawnRelationDefOf.Lover)
            .Position(recipient.Position)
            .Do(p => p.interactions.TryInteractWith(recipient, InteractionDefOf.RomanceAttempt))
            .CreateSingle();

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.NewAffair,
            Description = "[PAWN] and [Ex] were no longer in a relationship after [PAWN] had an affair with [NewLover].",
        };
        Expect.That(initiator).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [recipient, cuckold2] }));
        Expect.That(recipient).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [initiator, cuckold1] }));
        Expect.That(cuckold1).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [initiator, recipient] }));
        Expect.That(cuckold2).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [initiator, recipient] }));
        
        return () => NearDebugSettings.ForceRomanceSuccess = false;
    }

    public Action TestAffair2(TestScenario scenario)
    {
        NearDebugSettings.ForceRomanceSuccess = true;

        var cuckold1 = scenario.Pawn().Colonist().CreateSingle();
        var cuckold2 = scenario.Pawn().Colonist().CreateSingle();
        var recipient = scenario.Pawn()
            .Colonist()
            .SetRelation(cuckold1, PawnRelationDefOf.Spouse)
            .CreateSingle();
        var initiator = scenario.Pawn()
            .Colonist()
            .SetRelation(cuckold2, PawnRelationDefOf.Spouse)
            .Position(recipient.Position)
            .Do(p => p.interactions.TryInteractWith(recipient, InteractionDefOf.RomanceAttempt))
            .CreateSingle();

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.NewAffair,
            Description = "[PAWN], who married to [Ex], began an affair with [NewLover].",
        };
        Expect.That(initiator).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [recipient, cuckold2] }));
        Expect.That(recipient).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [initiator, cuckold1] }));
        Expect.That(cuckold1).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [initiator, recipient] }));
        Expect.That(cuckold2).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [initiator, recipient] }));
        
        return () => NearDebugSettings.ForceRomanceSuccess = false;
    }
}
