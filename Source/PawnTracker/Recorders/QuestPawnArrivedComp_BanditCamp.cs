using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using RimWorld.Planet;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class QuestPawnArrivedComp_BanditCamp : QuestPawnArrivedComp
{
    public override bool Match(Quest quest) =>
        quest.root.defName is nameof(Extra.QuestScriptDefOf.OpportunitySite_BanditCamp) or nameof(Extra.QuestScriptDefOf.Mission_BanditCamp);

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var worldObject = QuestHelper.GetWorldObject<WorldObject>(input.Quest);
        return builder.AddRule("WorldObject", worldObject.ColoredLabel, addSubsymbols: true);
    }

    // QuestNode_GiveRewards
    public void Test(TestScenario scenario)
    {
        var (quest, rewardPawn) = QuestPawnArrivedRecorder.SetupQuestWithReward(scenario, Extra.QuestScriptDefOf.OpportunitySite_BanditCamp);
        var site = QuestHelper.GetWorldObject<Site>(quest);
        var pawn = scenario.Pawn().Colonist().CreateSingle();

        scenario.Caravan([pawn]).VisitSite(site).KillAllEnemies().Execute();
        
        Expect.That(rewardPawn).Eventually().ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.QuestPawnArrived,
            Description = "[PAWN] joined the colony as a reward from [Faction] for clearing out the bandit camp.",
            Quest = quest,
        });
    }
}
