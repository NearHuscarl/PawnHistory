using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class OfferHelpRecorder : RecorderBase<OfferHelpEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<OfferHelpEvent>(CreateRecord);
    }

    public override void CreateRecord(OfferHelpEvent e)
    {
        var recordDef = HistoryRecordDefOf.OfferHelp;
        var desc = recordDef.Description(e.Refugee, "Refugee")
            .WithPlayerFaction()
            .AddRule("Rescuer", e.Rescuer)
            .Resolve();

        if (ShouldRecord(e.Rescuer))
            AddRecord(recordDef, e.Rescuer, desc, [e.Refugee], quest: e.Quest);
        if (ShouldRecord(e.Refugee))
            AddRecord(recordDef, e.Refugee, desc, [e.Rescuer], quest: e.Quest);
    }

    public void Test(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.OpportunitySite_DownedRefugee).Execute();
        var site = QuestHelper.GetWorldObject<Site>(quest);
        var rescuer = scenario.Pawn().Colonist().CreateSingle();

        Expect.Assertions(2);

        scenario.Caravan([rescuer])
            .VisitSite(site)
            .OnMapGenerated(e =>
            {
                var refugee = QuestHelper.GetPawnReward(quest);
                refugee.mindState.JoinColonyBecauseRescuedBy(rescuer);

                var expected = new ExpectedHistoryRecord
                {
                    Def = HistoryRecordDefOf.OfferHelp,
                    Description = "[Refugee] joined the colony after being rescued by [Rescuer].",
                    Quest = quest,
                };
                Expect.That(rescuer).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [refugee] }));
                Expect.That(refugee).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [rescuer] }));
            })
            .Execute();
    }
}
