using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class SocialFightRecorder : RecorderBase<SocialFightRecorder.Input>
{
    public record Input(Pawn Initiator, Pawn Recipient, string initiatorPov, string recipientPov);

    public override void Register()
    {
        GameEventBus.Subscribe<SocialFightStartedEvent>(e =>
        {
            var initiatorPov = e.InteractionEntry.ToGameStringFromPOV(e.Initiator);
            var recipientPov = e.InteractionEntry.ToGameStringFromPOV(e.Recipient);

            CreateRecord(new Input(e.Initiator, e.Recipient, initiatorPov, recipientPov));
        });
    }

    public override void CreateRecord(Input input)
    {
        var (initiator, recipient, initiatorPov, recipientPov) = input;

        if (ShouldRecord(initiator))
            AddRecord(HistoryRecordDefOf.SocialFight, initiator, initiatorPov, [recipient]);
        if (ShouldRecord(recipient))
            AddRecord(HistoryRecordDefOf.SocialFight, recipient, recipientPov, [initiator]);
    }

    [SkipTest]
    public void Test(TestScenario scenario)
    {
        var oldDebugValue = DebugSettings.alwaysSocialFight;
        DebugSettings.alwaysSocialFight = true;

        try
        {
            scenario.Pawn(8)
                .ThatMatches(ShouldRecord)
                .GroupTogether()
                .Do((p, i, pawns) => p.interactions.TryInteractWith(pawns[(i + 1) % pawns.Count], InteractionDefOf.Insult))
                .Execute();
        }
        finally
        {
            DebugSettings.alwaysSocialFight = oldDebugValue;
        }
    }
}
