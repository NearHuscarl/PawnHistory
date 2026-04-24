using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class QuestPawnArrivedComp_Bestowing : QuestPawnArrivedComp
{
    public override bool Match(Quest quest) => quest.root.defName == nameof(QuestScriptDefOf.BestowingCeremony);

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, Quest quest, Pawn pawn, List<Pawn> questPawns)
    {
        var bestowingPart = quest.GetFirstPartOfType<QuestPart_Bestowing_TargetChangedTitle>();
        return builder.AddRule("Recipient", bestowingPart.pawn)
            .AddRule("NewTitle", bestowingPart.currentTitle);
    }
    
    public override IEnumerable<Thing> GetConcerns(Quest quest, List<Pawn> questPawns)
    {
        var bestowingPart = quest.GetFirstPartOfType<QuestPart_Bestowing_TargetChangedTitle>();
        yield return bestowingPart.bestower;
        yield return bestowingPart.pawn;
    }

    [RequiresRoyalty]
    public void Test(TestScenario scenario)
    {
        Expect.Assertions(2);
        
        GameEventBus.SubscribeOnce<QuestPawnArrivedEvent>(e =>
        {
            Expect.ThatAll(e.Pawns).ToHaveHistoryRecord("[PAWN] arrived at [PlayerSettlement] to perform a bestowing ceremony, granting [Recipient] the title of [NewTitle].", HistoryRecordDefOf.QuestPawnArrived);
            Expect.ThatAll(e.Pawns).ToHaveHistoryRecordQuest(e.Quest, HistoryRecordDefOf.QuestPawnArrived);
        });
        
        TitleInheritedRecorder.SetupInheritance(scenario, RoyalTitleDefOf.Count);
        var quest = Find.QuestManager.ActiveQuestsListForReading.LastOrDefault();
        scenario.Quest(quest).Execute();
    }
}
