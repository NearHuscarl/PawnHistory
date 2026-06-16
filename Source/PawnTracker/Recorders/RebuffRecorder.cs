using System;
using System.Linq;
using PawnHistory.Source.DebugTools;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RebuffRecorder : RecorderBase<RebuffEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<RebuffEvent>(CreateRecord);
    }

    public override void CreateRecord(RebuffEvent e)
    {
        var recordDef = HistoryRecordDefOf.Rebuff;
        var romanceAttemptText = e.LogEntryText.Split('.').Select(p => p.Trim()).FirstOrDefault(p => !p.NullOrEmpty());
        var desc = recordDef.Description(e.Initiator, "Initiator")
            .IncludePawnGrammar()
            .AddRule("Recipient", e.Recipient, addSubsymbols: true)
            .AddRule("InteractionLog", romanceAttemptText)
            .Resolve();

        if (ShouldRecord(e.Initiator) && !IsTooSoonToRecordAgain(e.Initiator, recordDef, .5f))
            AddRecord(recordDef, e.Initiator, desc, [e.Recipient]);

        if (ShouldRecord(e.Recipient) && !IsTooSoonToRecordAgain(e.Recipient, recordDef, .5f))
            AddRecord(recordDef, e.Recipient, desc, [e.Initiator]);
    }

    public Action Test(TestScenario scenario)
    {
        NearDebugSettings.ForceRomanceRejection = true;

        var recipient = scenario.Pawn()
            .Colonist()
            .CreateSingle();
        var initiator = scenario.Pawn()
            .Colonist()
            .Position(recipient.Position) 
            .Do(p => p.interactions.TryInteractWith(recipient, InteractionDefOf.RomanceAttempt))
            .CreateSingle();

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.Rebuff,
            Description = "[Recipient] rejected [Initiator]'s romantic advance.",
        };
        Expect.That(initiator).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [recipient] }));
        Expect.That(recipient).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [initiator] }));

        return () => NearDebugSettings.ForceRomanceRejection = false;
    }
}
