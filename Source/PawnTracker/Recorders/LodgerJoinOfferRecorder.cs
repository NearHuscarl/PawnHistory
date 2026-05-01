using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class LodgerJoinOfferRecorder : RecorderBase<LodgerJoinOfferAcceptedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<LodgerJoinOfferAcceptedEvent>(CreateRecord);
    }

    public override void CreateRecord(LodgerJoinOfferAcceptedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var recordDef = HistoryRecordDefOf.LodgerJoinOffer;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .WithPlayerSettlement(e.Pawn.MapHeld.Parent)
            .AddRule("Quest", e.Quest.name.Colorize(ColoredText.GeneColor))
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc, quest: e.Quest);
    }

    [RequiresRoyalty]
    public void TestHospitalityRefugee(TestScenario scenario)
    {
        TestJoinOffer(scenario, QuestScriptDefOf.Hospitality_Refugee);
    }

    // TODO: test biotech dlc
    [RequiresBiotech]
    public void TestSanguophageMeetingHost(TestScenario scenario)
    {
        TestJoinOffer(scenario, Extra.QuestScriptDefOf.SanguophageMeetingHost);
    }

    private static void TestJoinOffer(TestScenario scenario, QuestScriptDef questScript)
    {
        scenario.Pawn().Colonist().CreateSingle();

        var quest = scenario.Quest(questScript).Execute();
        var joinOffer = quest.GetFirstPartOfType<QuestPart_PawnJoinOffer>();
        Accessor.QuestPart_PawnJoinOffer.SendLetter(joinOffer);

        var letter = scenario.Letter<ChoiceLetter_AcceptVisitors>().Accept().Execute();
        var joiner = letter.pawns.Single();

        Expect.That(joiner).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.LodgerJoinOffer,
            Description = "During [Quest] quest, [PAWN] believed that [He] was happy here, and wished to join the colony permanently. The colony welcomed [Him] in.",
            Quest = quest,
        });
    }
}
