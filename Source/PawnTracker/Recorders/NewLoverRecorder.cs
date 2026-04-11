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
        scenario.SpeedUp();

        var recipient = scenario.Pawn()
            .Colonist()
            .CreateSingle();
        var initiator = scenario.Pawn()
            .Colonist()
            .WithPosition(recipient.Position) 
            .Do((p, i, pawns) => p.interactions.TryInteractWith(recipient, InteractionDefOf.RomanceAttempt))
            .CreateSingle();

        Expect.That(initiator)
            .Eventually()
            .ToHaveHistoryRecord("[InteractionLog]. [RomanceSuccessPrefix] [Initiator]'s lover.", HistoryRecordDefOf.NewLover);
        Expect.That(recipient)
            .Eventually()
            .ToHaveHistoryRecord("[InteractionLog]. [RomanceSuccessPrefix] [Initiator]'s lover.", HistoryRecordDefOf.NewLover);

        return () =>
        {
            NearDebugSettings.ForceRomanceSuccess = false;
            scenario.SlowDown();
        };
    }

    public Action TestAffair(TestScenario scenario)
    {
        NearDebugSettings.ForceRomanceSuccess = true;
        scenario.SpeedUp();

        var cuckold = scenario.Pawn() .Colonist() .CreateSingle();
        var cuckold2 = scenario.Pawn() .Colonist() .CreateSingle();
        var recipient = scenario.Pawn()
            .Colonist()
            .SetRelation(cuckold, PawnRelationDefOf.Lover)
            .CreateSingle();
        var initiator = scenario.Pawn()
            .Colonist()
            .SetRelation(cuckold2, PawnRelationDefOf.Lover)
            .WithPosition(recipient.Position)
            .Do((p, i, pawns) => p.interactions.TryInteractWith(recipient, InteractionDefOf.RomanceAttempt))
            .CreateSingle();

        Expect.That(initiator)
            .Eventually()
            .ToHaveHistoryRecord("[InteractionLog]. [RomanceSuccessPrefix] [Initiator]'s lover.", HistoryRecordDefOf.NewLover);
        Expect.That(recipient)
            .Eventually()
            .ToHaveHistoryRecord("[InteractionLog]. [RomanceSuccessPrefix] [Initiator]'s lover.", HistoryRecordDefOf.NewLover);
        Expect.That(cuckold)
            .Eventually()
            .ToHaveHistoryRecord("[PAWN] and [Ex] were no longer in a relationship after [PAWN] had an affair with [NewLover].", HistoryRecordDefOf.NewAffair);
        Expect.That(cuckold2)
            .Eventually()
            .ToHaveHistoryRecord("[PAWN] and [Ex] were no longer in a relationship after [PAWN] had an affair with [NewLover].", HistoryRecordDefOf.NewAffair);
        
        return () =>
        {
            NearDebugSettings.ForceRomanceSuccess = false;
            scenario.SlowDown();
        };
    }

    public Action TestAffair2(TestScenario scenario)
    {
        NearDebugSettings.ForceRomanceSuccess = true;
        scenario.SpeedUp();

        var cuckold = scenario.Pawn() .Colonist() .CreateSingle();
        var cuckold2 = scenario.Pawn() .Colonist() .CreateSingle();
        var recipient = scenario.Pawn()
            .Colonist()
            .SetRelation(cuckold, PawnRelationDefOf.Spouse)
            .CreateSingle();
        var initiator = scenario.Pawn()
            .Colonist()
            .SetRelation(cuckold2, PawnRelationDefOf.Spouse)
            .WithPosition(recipient.Position)
            .Do((p, i, pawns) => p.interactions.TryInteractWith(recipient, InteractionDefOf.RomanceAttempt))
            .CreateSingle();

        Expect.That(initiator)
            .Eventually()
            .ToHaveHistoryRecord("[InteractionLog]. [RomanceSuccessPrefix] [Initiator]'s lover.", HistoryRecordDefOf.NewLover);
        Expect.That(recipient)
            .Eventually()
            .ToHaveHistoryRecord("[InteractionLog]. [RomanceSuccessPrefix] [Initiator]'s lover.", HistoryRecordDefOf.NewLover);
        Expect.That(cuckold)
            .Eventually()
            .ToHaveHistoryRecord("[PAWN], who married to [Ex], began an affair with [NewLover].", HistoryRecordDefOf.NewAffair);
        Expect.That(cuckold2)
            .Eventually()
            .ToHaveHistoryRecord("[PAWN], who married to [Ex], began an affair with [NewLover].", HistoryRecordDefOf.NewAffair);
        
        return () =>
        {
            NearDebugSettings.ForceRomanceSuccess = false;
            scenario.SlowDown();
        };
    }
}
