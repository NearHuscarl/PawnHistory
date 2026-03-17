using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class SocialFightRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<SocialFightStartedEvent>(HandleSocialFightEvent);
    }

    private void HandleSocialFightEvent(SocialFightStartedEvent e)
    {
        var initiatorPov = e.InteractionEntry.ToGameStringFromPOV(e.Initiator);
        var recipientPov = e.InteractionEntry.ToGameStringFromPOV(e.Recipient);

        CreateRecord(e.Initiator, initiatorPov, [e.Recipient]);
        CreateRecord(e.Recipient, recipientPov, [e.Initiator]);
    }

    private void CreateRecord(Pawn pawn, string description, IEnumerable<Pawn> concerns)
    {
        if (!ShouldRecord(pawn))
            return;

        AddRecord(HistoryRecordDefOf.SocialFight, pawn, description, concerns);
    }

    public override void Test(TestScenario scenario)
    {
        var oldDebugValue = DebugSettings.alwaysSocialFight;
        DebugSettings.alwaysSocialFight = true;

        try
        {
            scenario.Pawn(8)
                .ThatMatches(ShouldRecord)
                .GroupTogether()
                .Do((p, i, pawns) => p.interactions.TryInteractWith(pawns[(i + 1) % pawns.Count], InteractionDefOf.Insult))
                .Create();
        }
        finally
        {
            DebugSettings.alwaysSocialFight = oldDebugValue;
        }
    }
}
