using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class QuestPawnArrivedComp_RoyalAscent : QuestPawnArrivedComp
{
    public override bool Match(Quest quest) => ModsConfig.RoyaltyActive && quest.root == Extra.QuestScriptDefOf.EndGame_RoyalAscent;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var asker = input.QuestAsker;
        return builder.AddRule("AskerTitle", asker.royalty.MainTitle())
            .AddRule("EmpireFaction", asker.HomeFaction);
    }

    public override IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        yield return input.QuestAsker;
    }

    [RequiresRoyalty]
    public void Test(TestScenario scenario)
    {
        Expect.Assertions(2);
        
        GameEventBus.SubscribeOnce<QuestPawnArrivedEvent>(e =>
        {
            var pawns = e.Pawns;
            var stellarch = pawns.FirstOrDefault(p => p.kindDef == Extra.PawnKindDefOf.Empire_Royal_Stellarch);
            var guards = pawns.Where(p => p != stellarch).ToList();
            
            Expect.That(stellarch).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.QuestPawnArrived,
                Description = "[PAWN] arrived at the colony for a customary royal visit, offering passage to the Imperial flotilla if [He] was properly hosted.",
                Quest = e.Quest,
                Concerns = [..guards]
            });
            Expect.ThatAll(guards).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.QuestPawnArrived,
                Description = "[PAWN] accompanied [Asker], the Stellarch of [EmpireFaction], as a personal guard during [His] visit to [PlayerSettlement].",
                Quest = e.Quest,
                ConcernAtLeast = [stellarch]
            });
        });

        scenario.Quest(Extra.QuestScriptDefOf.EndGame_RoyalAscent).Execute();
    }
}
