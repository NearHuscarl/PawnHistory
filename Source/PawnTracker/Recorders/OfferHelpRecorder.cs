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
        
        // OfferHelp is only used in the OpportunitySite_DownedRefugee quest atm
        var quest = Find.QuestManager.QuestsListForReading.LastOrDefault(q => !q.hidden && QuestHelper.IsReward(q, e.Refugee));

        if (ShouldRecord(e.Rescuer))
            AddRecord(recordDef, e.Rescuer, desc, [e.Refugee], quest: quest);
        if (ShouldRecord(e.Refugee))
            AddRecord(recordDef, e.Refugee, desc, [e.Rescuer], quest: quest);
    }

    public void Test(TestScenario scenario)
    {
        var quest = scenario.Quest(DefLookup.QuestScript.OpportunitySite_DownedRefugee).Execute();
        var site = QuestHelper.GetWorldObject<Site>(quest);
        var rescuer = scenario.Pawn().Colonist().CreateSingle();

        Expect.Assertions(2);

        scenario.Caravan([rescuer])
            .VisitSite(site)
            .OnMapGenerated(e =>
            {
                var refugee = QuestHelper.GetPawnReward(quest);
                refugee.mindState.JoinColonyBecauseRescuedBy(rescuer);

                Expect.That(rescuer).ToHaveHistoryRecord(new ExpectedHistoryRecord
                {
                    Def = HistoryRecordDefOf.OfferHelp,
                    Description = "[Refugee] joined the colony after being rescued by [Rescuer].",
                    Concerns = [refugee],
                    Quest = quest,
                });
                Expect.That(refugee).ToHaveHistoryRecord(new ExpectedHistoryRecord
                {
                    Def = HistoryRecordDefOf.OfferHelp,
                    Description = "[Refugee] joined the colony after being rescued by [Rescuer].",
                    Concerns = [rescuer],
                    Quest = quest,
                });
            })
            .Execute();
    }
}
