using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using PawnHistory.Source.Helper;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class QuestPawnArrivedRecorder : RecorderBase<QuestPawnArrivedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<QuestPawnArrivedEvent>(CreateRecord);
    }

    public override void CreateRecord(QuestPawnArrivedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var recordDef = HistoryRecordDefOf.QuestPawnArrived;
        var questScriptDef = e.Quest.root.defName;
        var asker = QuestHelper.GetAsker(e.Quest);
        var isReward = QuestHelper.IsReward(e.Quest, e.Pawn);
        var involvedFactions = e.Quest.InvolvedFactions.ToList();
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddRule("Quest", e.Quest.name.Colorize(ColoredText.GeneColor))
            .AddRule("Asker", asker)
            .AddRule("Faction", involvedFactions.Count > 0 ? involvedFactions[0] : null)
            .AddRule("Faction2", involvedFactions.Count > 1 ? involvedFactions[1] : null)
            .WithPlayerSettlement(e.Pawn.MapHeld?.Parent)
            .WithOthers(e.Group)
            .AddConstant("isEnemy", e.Pawn.Faction.HostileTo(Faction.OfPlayer))
            .AddConstant("quest", questScriptDef)
            .AddConstant("isReward", isReward)
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc, e.Group, quest: e.Quest);
    }
    
    public void TestWandererJoin(TestScenario scenario)
    {
        // IncidentWorker_GiveQuest
        scenario.Incident(DefLookup.Incident.WandererJoin).Execute();
        var letter = Find.LetterStack.LettersListForReading.OfType<ChoiceLetter_AcceptJoiner>().Last();
        letter.Choices.First().action(); // accept
        
        var pawns = QuestHelper.GetArrivalPawns();
        Expect.That(pawns.Last()).ToHaveHistoryRecord("A [PAWN_title] named [PAWN] arrived and joined the colony. [He] is willing to contribute, but refuses to leave voluntarily, claiming to have nowhere else to go.", HistoryRecordDefOf.QuestPawnArrived);
    }
    
    public void TestRefugeePodCrash(TestScenario scenario)
    {
        // IncidentWorker_GiveQuest
        scenario.Incident(DefLookup.Incident.RefugeePodCrash).Execute();
        var pawns = QuestHelper.GetArrivalPawns();
        Expect.That(pawns.Last()).ToHaveHistoryRecord("[PAWN_titleIndef] named [PAWN] was crashed in a nearby transport pod. [He] survived the impact but was badly wounded.", HistoryRecordDefOf.QuestPawnArrived);
    }
    
    public void TestRaidJoiner(TestScenario scenario)
    {
        scenario.Quest(DefLookup.QuestScript.ThreatReward_Raid_Joiner).Execute();
        scenario.ForwardTime(1f);
        
        Expect.Assertions(3);

        GameEventBus.SubscribeOnce<RaidStartedEvent>(e =>
        {
            var pawn = QuestHelper.GetArrivalPawns().First(p => p.Faction.IsPlayer);
            
            Expect.That(pawn).ToHaveHistoryRecord("[PAWN] joined the colony in exchange for safety while being pursued.", HistoryRecordDefOf.QuestPawnArrived);
            Expect.ThatAll(e.Pawns).ToHaveHistoryRecord("[RaidText]. [He] came to take [QuestAsker].", HistoryRecordDefOf.Raid);
            Expect.ThatAll(e.Pawns).ToHaveHistoryRecordConcern(pawn, HistoryRecordDefOf.Raid);
        });
    }
    
    [RequiresRoyalty]
    public void TestEmpireDeserter(TestScenario scenario)
    {
        scenario.Quest(DefLookup.QuestScript.Intro_Deserter).Execute();
        scenario.ForwardTime(1f);
        
        Expect.Assertions(3);

        GameEventBus.SubscribeOnce<RaidStartedEvent>(e =>
        {
            var pawn = QuestHelper.GetArrivalPawns().First(p => p.Faction.IsPlayer);
            
            Expect.That(pawn).ToHaveHistoryRecord("[PAWN] joined [PlayerSettlement] after deserting [Faction], while being hunted by a loyalty squad.", HistoryRecordDefOf.QuestPawnArrived);
            Expect.ThatAll(e.Pawns).ToHaveHistoryRecord("[RaidText]. [He] came to hunt down [QuestAsker].", HistoryRecordDefOf.Raid);
            Expect.ThatAll(e.Pawns).ToHaveHistoryRecordConcern(pawn, HistoryRecordDefOf.Raid);
        });
    }
    
    [RequiresRoyalty]
    public void TestHospitalityRefugee(TestScenario scenario)
    {
        scenario.Quest(QuestScriptDefOf.Hospitality_Refugee).Execute();
        var pawns = QuestHelper.GetArrivalPawns();
        
        Expect.ThatAll(pawns).ToHaveHistoryRecord("[PAWN][AndOthers] arrived at [PlayerSettlement] seeking shelter and a place to regroup, with nowhere else to go.", HistoryRecordDefOf.QuestPawnArrived);
    }

    [RequiresRoyalty]
    public Action TestShuttleCrashRescue(TestScenario scenario)
    {
        TestManager.Timeout = 100000000;
        scenario.SpeedUp();
        Expect.Assertions(2);
        
        scenario.Quest(DefLookup.QuestScript.ShuttleCrash_Rescue).Execute();

        // QuestNode_Root_ShuttleCrash_Rescue.QuestStartDelay
        TickDelayManager.Delay(120, () =>
        {
            var pawns = QuestHelper.GetArrivalPawns();
            Expect.ThatAll(pawns).ToHaveHistoryRecord("[PAWN] along with [n] others made an emergency landing at [PlayerSettlement] in a damaged shuttle.", HistoryRecordDefOf.QuestPawnArrived);
            
            scenario.ForwardTime(1f);
        });

        scenario.WaitUntil(() => QuestHelper.GetArrivalPawns().Any(p => p.Faction.HostileTo(Faction.OfPlayer)), () =>
        {
            var raiders = QuestHelper.GetArrivalPawns().Where(p => p.Faction.HostileTo(Faction.OfPlayer));
            Expect.ThatAll(raiders).ToHaveHistoryRecord("[PAWN] and [n] others from [Faction] attacked the crashed shuttle site.", HistoryRecordDefOf.QuestPawnArrived);
        });
        return () => scenario.SlowDown();
    }

    public void TestTradeRequest(TestScenario scenario)
    {
        var colonist = scenario.Pawn().Colonist().CreateSingle();
        var rewardPawn = scenario.Pawn().WorldPawn().CreateSingle(false);
        
        scenario.ForceRewardPawnInQuest = rewardPawn;

        var quest = scenario.Quest(DefLookup.QuestScript.TradeRequest)
            .ChooseReward(choice => choice.rewards.OfType<Reward_Pawn>().Any())
            .Execute();

        var tradePart = quest.GetFirstPartOfType<QuestPart_InitiateTradeRequest>();
        var gifts = scenario.Thing(tradePart.requestedThingDef).Stack(tradePart.requestedCount).Create();

        scenario.Caravan([colonist]).Give(gifts).FulfillTradeRequest(tradePart.settlement).Execute();
        
        Expect.That(rewardPawn).ToHaveHistoryRecord("[PAWN] joined the colony as a reward for fulfilling a trade request from [Faction].", HistoryRecordDefOf.QuestPawnArrived);
    }
}
