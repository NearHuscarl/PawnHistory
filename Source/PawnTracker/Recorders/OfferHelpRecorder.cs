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

        Expect.Assertions(4);

        scenario.Caravan([rescuer])
            .VisitSite(site)
            .OnMapGenerated(e =>
            {
                var refugee = QuestHelper.GetPawnReward(quest);
                refugee.mindState.JoinColonyBecauseRescuedBy(rescuer);

                Expect.That(rescuer).ToHaveHistoryRecord("[Refugee] joined the colony after being rescued by [Rescuer].", HistoryRecordDefOf.OfferHelp);
                Expect.That(refugee).ToHaveHistoryRecord("[Refugee] joined the colony after being rescued by [Rescuer].", HistoryRecordDefOf.OfferHelp);
                Expect.That(rescuer).ToHaveHistoryRecordConcern(refugee, HistoryRecordDefOf.OfferHelp);
                Expect.That(refugee).ToHaveHistoryRecordConcern(rescuer, HistoryRecordDefOf.OfferHelp);
            })
            .Execute();
    }
}
