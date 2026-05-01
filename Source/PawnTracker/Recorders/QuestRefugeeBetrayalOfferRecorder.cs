using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class QuestRefugeeBetrayalOfferRecorder : RecorderBase<QuestRefugeeBetrayalOfferEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<QuestRefugeeBetrayalOfferEvent>(CreateRecord);
    }

    public override void CreateRecord(QuestRefugeeBetrayalOfferEvent input)
    {
        var (factionOpponent, lodgers, refugeeFaction, quest, betrayalQuest) = input;

        if (!ShouldRecord(factionOpponent))
            return;

        var recordDef = HistoryRecordDefOf.QuestRefugeeBetrayalOffer;
        var acceptor = quest.AccepterPawn;
        var rewardThings = betrayalQuest.GetFirstPartOfType<QuestPart_DropPods>().Things.ToList();
        var rewardValue = rewardThings.Sum(t => t.MarketValue * t.stackCount);
        var desc = recordDef.Description(factionOpponent)
            .WithPlayerSettlement(lodgers.First().MapHeld.Parent)
            .AddRule("Acceptor", acceptor)
            .AddRule("RewardThings", LangUtility.FormatList(rewardThings, t => t.LabelNoCount.Colorize(t.DrawColor)))
            .AddRule("SilverCount", ConvertHelper.Convert<float>(rewardValue).ToStringMoney())
            .AddRule("RefugeeFaction", refugeeFaction)
            .AddRule("RefugeePawn", lodgers.First())
            .AddConstant("refugeeCount", lodgers.Count)
            .Resolve();

        AddRecord(recordDef, factionOpponent, desc, [acceptor, ..lodgers], quest: betrayalQuest);
        if (ShouldRecord(acceptor))
            AddRecord(recordDef, acceptor, desc, [factionOpponent, ..lodgers], quest: betrayalQuest);
    }

    [RequiresRoyalty]
    public void Test(TestScenario scenario)
    {
        scenario.Pawn().Colonist().CreateSingle();

        var quest = scenario.Quest(QuestScriptDefOf.Hospitality_Refugee, 2000).Execute();
        var lodgers = QuestHelper.GetArrivalPawns(quest);
        var refugeeExtraFaction = quest.GetFirstPartOfType<QuestPart_ExtraFaction>();
        var factionOpponent = scenario.Pawn().FactionLeader(Faction.OfNonHostile).CreateSingle();
        var addQuestPart = new QuestPart_AddQuest_RefugeeBetrayal
        {
            acceptee = quest.AccepterPawn,
            asker = lodgers.First(),
            factionOpponent = factionOpponent,
            inSignal = "test.refugeeBetrayalOffer",
            lodgers = lodgers,
            mapParent = Find.AnyPlayerHomeMap.Parent,
            parent = quest,
            refugeeFaction = refugeeExtraFaction.extraFaction,
            sendAvailableLetter = true,
        };
        quest.AddPart(addQuestPart);
        addQuestPart.Notify_QuestSignalReceived(new Signal(addQuestPart.inSignal));

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.QuestRefugeeBetrayalOffer,
            Description = "[PAWN] asked [Acceptor] to betray and kill all refugees from [RefugeeFaction], who had taken shelter at the colony, in exchange for a reward: [RewardThings] ([SilverCount]).",
            Quest = Find.QuestManager.QuestsListForReading.Last(),
        };
        Expect.That(factionOpponent).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = lodgers.Concat(quest.AccepterPawn).Cast<Thing>().ToList() }));
        Expect.That(quest.AccepterPawn).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = lodgers.Concat(factionOpponent).Cast<Thing>().ToList() }));
    }
}
