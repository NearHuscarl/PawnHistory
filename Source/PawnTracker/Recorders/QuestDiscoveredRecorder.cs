using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using PawnHistory.Source.Helper;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class QuestDiscoveredRecorder : RecorderBase<QuestDiscoveredEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<QuestDiscoveredEvent>(CreateRecord);
    }

    public override void CreateRecord(QuestDiscoveredEvent input)
    {
        var (discoverer, quest, source, sourceThing, sourcePawn ) = input;
        
        if (!ShouldRecord(discoverer))
            return;

        var recordDef = HistoryRecordDefOf.QuestDiscovered;
        var builder = recordDef.Description(discoverer)
            .IncludePawnGrammar()
            .AddRule("Quest", quest.name.Colorize(ColoredText.GeneColor))
            .AddRule("SourcePawn", sourcePawn)
            .AddConstant("source", source);

        if (source == QuestDiscoveredSource.Book && sourceThing is Book book)
            builder.AddRule("SourceThing", book.Title.Colorize(ColoredText.SubtleGrayColor));
        else
            builder.AddRule("SourceThing", sourceThing?.def, addSubsymbols: true);
        var desc = builder.Resolve();
        
        AddRecord(recordDef, discoverer, desc, [sourcePawn, sourceThing], quest: quest);
        if (ShouldRecord(sourcePawn))
            AddRecord(recordDef, sourcePawn, desc, [discoverer, sourceThing], quest: quest);
    }

    [RequiresOdyssey]
    public void TestBook(TestScenario scenario)
    {
        var reader = scenario.Pawn().Colonist().CreateSingle();
        var book = scenario.Thing(ThingDefOf.Map).CreateSingle();
        var doer = book.TryGetComp<CompBook>().Doers.OfType<BookOutcomeDoer_GiveQuest>().SingleOrDefault();
        
        Accessor.BookOutcomeDoer_GiveQuest.GenerateQuest(doer, reader);
        var quest = Find.QuestManager.QuestsListForReading.Last();

        Expect.That(reader).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.QuestDiscovered,
            Description = "While reading [Book], [PAWN] discovered [Quest].",
            Concerns = [book],
            Quest = quest,
        });
    }

    [RequiresOdyssey]
    public void TestTrader(TestScenario scenario)
    {
        var negotiator = scenario.Pawn().Colonist().CreateSingle();
        var trader = scenario.Pawn().WithFaction(Faction.OfNonHostile).CreateSingle(false);
        trader.mindState.hasQuest = true;

        TradeUtility.ReceiveQuestFromTrader(trader, negotiator);
        var quest = Find.QuestManager.QuestsListForReading.Last();

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.QuestDiscovered,
            Description = "While talking with the trader [Trader], [PAWN] learned about [Quest].",
            Quest = quest,
        };
        Expect.That(negotiator).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [trader] }));
        Expect.That(trader).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [negotiator] }));
    }

    [RequiresOdyssey]
    public void TestUplink(TestScenario scenario)
    {
        var hacker = scenario.Pawn().Colonist().CreateSingle();
        var uplink = scenario.Thing(Extra.ThingDefOf.AncientUplink).CreateSingle();
        var comp = uplink.TryGetComp<CompAncientUplink>();

        comp.Notify_Hacked(hacker);
        var quest = Find.QuestManager.QuestsListForReading.Last();

        Expect.That(hacker).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.QuestDiscovered,
            Description = "[PAWN] hacked the ancient uplink and discovered [Quest].",
            Concerns = [uplink],
            Quest = quest,
        });
    }

    [RequiresIdeology]
    [RequiresOdyssey]
    public void TestBeggar(TestScenario scenario)
    {
        var receiver = scenario.Pawn().WithFaction(Faction.OfNonHostile).CreateSingle(false);
        var quest = scenario.Quest(Extra.QuestScriptDefOf.Beggars).Execute();
        var giver = QuestHelper.GetArrivalPawns(quest).First();
        // QuestNode_Root_Beggars
        var addGiverQuest = new QuestPart_AddGiverQuest
        {
            inSignal = "test.beggar.received",
            questScript = QuestScriptDefOf.OpportunitySite_ItemStash,
            discoveryMethodTranslationKey = "QuestDiscoveredFromBeggar",
            points = 500f,
            sendAvailableLetter = true,
        };
        quest.AddPart(addGiverQuest);

        // JobDriver_GiveToPawn.cs
        addGiverQuest.Notify_QuestSignalReceived(new Signal(addGiverQuest.inSignal, giver.Named(SignalArgsNames.Giver), receiver.Named(SignalArgsNames.Receiver)));
        
        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.QuestDiscovered,
            Description = "[PAWN] learned about [Quest] after the beggar [SourcePawn] shared a secret in gratitude for the colony's charity.",
            Quest = Find.QuestManager.QuestsListForReading.Last(),
        };
        Expect.That(giver).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [receiver] }));
        Expect.That(receiver).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [giver] }));
    }
}
