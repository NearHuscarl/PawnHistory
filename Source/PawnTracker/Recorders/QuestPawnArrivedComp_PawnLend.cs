using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class QuestPawnArrivedComp_PawnLend : QuestPawnArrivedComp
{
    public override bool Match(Quest quest) => quest.root.defName == nameof(Extra.QuestScriptDefOf.PawnLend);

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, Quest quest, Pawn pawn, List<Pawn> questPawns)
    {
        var daysLent = quest.GetFirstPartOfType<QuestPart_LendColonistsToFaction>().returnLentColonistsInTicks  / GenDate.TicksPerDay;
        var requiredCount = quest.GetFirstPartOfType<QuestPart_SetupTransportShip>().transportShip.ShuttleComp.requiredColonistCount;
        return builder.AddRule("DaysLent", daysLent)
            .AddRule("RequiredCount", requiredCount.ToString());
    }

    // QuestNode_GiveRewards
    [RequiresRoyalty]
    public void Test(TestScenario scenario)
    {
        TestManager.Timeout = 9999999;

        Expect.Assertions(1);
        var (quest, rewardPawn) = QuestPawnArrivedRecorder.SetupQuestWithReward(scenario, Extra.QuestScriptDefOf.PawnLend);
        var shuttle = quest.GetFirstPartOfType<QuestPart_SetupTransportShip>().transportShip;
        var requiredCount = shuttle.ShuttleComp.requiredColonistCount;
        var colonists = scenario.Pawn(requiredCount).Colonist().Execute();
        
        scenario.SpeedUp();
        scenario.ForwardTicks(3500); // QuestNode_ShuttleDelay
        scenario.WaitUntil(
            () => shuttle.ShipExistsAndIsSpawned,
            () =>
            {
                scenario.Shuttle(shuttle).Load(colonists).Launch();
                Expect.That(rewardPawn).Eventually().ToHaveHistoryRecord(new ExpectedHistoryRecord
                {
                    Def = HistoryRecordDefOf.QuestPawnArrived,
                    Description = "[PAWN] joined [PlayerSettlement] as a reward after [PlayerSettlement] lent [RequiredCount] colonists to [Faction] for [n] days.",
                    Quest = quest,
                });
            });
    }
}
