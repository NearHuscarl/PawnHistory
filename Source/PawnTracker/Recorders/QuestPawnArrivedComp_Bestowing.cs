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

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var bestowingPart = input.Quest.GetFirstPartOfType<QuestPart_Bestowing_TargetChangedTitle>();
        return builder
            .AddRule("Count", input.Pawns.Where(p => p.kindDef != PawnKindDefOf.Empire_Royal_Bestower).ToList().Count - 1)
            .AddRule("Recipient", bestowingPart.pawn, addSubsymbols: true)
            .AddRule("NewTitle", bestowingPart.currentTitle);
    }
    
    public override IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        var bestowingPart = input.Quest.GetFirstPartOfType<QuestPart_Bestowing_TargetChangedTitle>();
        yield return bestowingPart.bestower;
        yield return bestowingPart.pawn;
    }

    [RequiresRoyalty]
    public void Test(TestScenario scenario)
    {
        Expect.Assertions(2);
        
        GameEventBus.SubscribeOnce<QuestPawnArrivedEvent>(e =>
        {
            var bestower = e.Pawns.FirstOrDefault(p => p.kindDef == PawnKindDefOf.Empire_Royal_Bestower);
            var guards = e.Pawns.Except(bestower);
            Expect.That(bestower).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.QuestPawnArrived,
                Description = "[PAWN] arrived at the colony to perform a bestowing ceremony, granting [Recipient] the title of [NewTitle].",
                Quest = e.Quest,
            });
            Expect.ThatAll(guards).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.QuestPawnArrived,
                Description = "[PAWN] along with [n] others accompanied the bestower to [PlayerSettlement] for [Recipient]'s bestowing ceremony, where [Recipient_pronoun] received the title of [NewTitle].",
                Quest = e.Quest,
            });
        });
        
        TitleInheritedRecorder.SetupInheritance(scenario, RoyalTitleDefOf.Count);
        var quest = Find.QuestManager.ActiveQuestsListForReading.LastOrDefault();
        scenario.Quest(quest).Execute();
    }
}
