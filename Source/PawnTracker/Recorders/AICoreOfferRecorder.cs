using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using System;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class AICoreOfferRecorder : RecorderBase<Pawn>
{
    private static readonly string LetterLabelAICoreOffer = "LetterLabelAICoreOffer".Translate();

    public override void Register()
    {
        GameEventBus.Subscribe<ReceiveLetterEvent>(e =>
        {
            if (e.Label.Resolve() != LetterLabelAICoreOffer)
                return;

            var leader = e.Faction?.leader;
            if (leader == null)
                return;

            CreateRecord(leader);
        });
    }

    public override void CreateRecord(Pawn leader)
    {
        if (!ShouldRecord(leader))
            return;

        var recordDef = HistoryRecordDefOf.AICoreOffer;
        var desc = recordDef.Description(leader)
            .AddRule("Faction", leader.Faction)
            .Resolve();

        AddRecord(recordDef, leader, desc);
    }

    public void Test(TestScenario scenario)
    {
        var notification = Current.Game.GetComponent<GameComponent_OnetimeNotification>();
        
        Accessor.GameComponent_OnetimeNotification.SendAICoreRequestReminder(notification) = true;
        Find.ResearchManager.DebugSetAllProjectsFinished();
        Find.TickManager.DebugSetTicksGame(2000);

        ReceiveLetterEvent received = null;
        GameEventBus.SubscribeOnce<ReceiveLetterEvent>(e =>
        {
            if (e.Label.Resolve() != LetterLabelAICoreOffer)
                return;

            received = e;
        });

        for (var i = 0; i < 2000 && received == null; i++)
            notification.GameComponentTick();

        if (received == null)
            throw new InvalidOperationException("Failed to trigger the AICore offer letter.");
        
        Expect.That(received.Faction.leader).ToHaveHistoryRecord("[PAWN] from [Faction] contacted the colony with an offer of information about a persona core that can be used to build a spaceship.", HistoryRecordDefOf.AICoreOffer);
    }
}
