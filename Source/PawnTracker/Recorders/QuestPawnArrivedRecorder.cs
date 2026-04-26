using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using PawnHistory.Source.Helper;
using RimWorld;
using System;
using System.Linq;
using RimWorld.Planet;
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
        var recordDef = HistoryRecordDefOf.QuestPawnArrived;
        var questScriptDef = e.Quest.root.defName;
        var involvedFactions = e.Quest.InvolvedFactions.ToList();
        var questPawns = QuestHelper.GetQuestPawns(e.Quest).ToList();

        foreach (var pawn in e.Pawns)
        {
            if (!ShouldRecord(pawn))
                continue;

            var isReward = QuestHelper.IsReward(e.Quest, pawn);
            var builder = recordDef.Description(pawn)
                .IncludePawnGrammar()
                .AddRule("Quest", e.Quest.name.Colorize(ColoredText.GeneColor))
                .AddRule("Faction", involvedFactions.Count > 0 ? involvedFactions[0] : null)
                .WithPlayerSettlement(pawn.MapHeld?.Parent)
                .WithOthers(e.Pawns)
                .AddConstant("isEnemy", pawn.HostileTo(Faction.OfPlayer))
                .AddConstant("quest", questScriptDef)
                .AddConstant("isReward", isReward);
            var concerns = e.Pawns.Cast<Thing>();

            foreach (var comp in Comps.OfType<QuestPawnArrivedComp>())
            {
                if (!comp.Match(e.Quest))
                    continue;

                builder = comp.BuildGrammarRequest(builder, e.Quest, pawn, questPawns);
                concerns = concerns.Concat(comp.GetConcerns(e.Quest, questPawns));
            }
            
            AddRecord(recordDef, pawn, builder.Resolve(), concerns, quest: e.Quest);
        }
    }
    
    public static void AssertArrived(Quest quest, string description, Func<Pawn, bool> filter = null)
    {
        var pawns = QuestHelper.GetArrivalPawns(quest);
        if (filter != null)
            pawns = pawns.Where(filter).ToList();

        Expect.ThatAll(pawns).ToHaveHistoryRecord(description, HistoryRecordDefOf.QuestPawnArrived);
        Expect.ThatAll(pawns).ToHaveHistoryRecordQuest(quest, HistoryRecordDefOf.QuestPawnArrived);
    }
    
    // IncidentWorker_GiveQuest, QuestPart_PawnsArrive, quest.DropPods()
    public void TestIncident(TestScenario scenario)
    {
        scenario.Incident(DefLookup.Incident.WandererJoin).Execute();
        scenario.Letter<ChoiceLetter_AcceptJoiner>().Accept();
        
        var pawns1 = QuestHelper.GetArrivalPawns();
        Expect.That(pawns1.Last()).ToHaveHistoryRecord("A [PAWN_title] named [PAWN] arrived and joined the colony. [He] is willing to contribute, but refuses to leave voluntarily, claiming to have nowhere else to go.", HistoryRecordDefOf.QuestPawnArrived);
        
        scenario.Incident(DefLookup.Incident.RefugeePodCrash).Execute();
        var pawns2 = QuestHelper.GetArrivalPawns();
        Expect.That(pawns2.Last()).ToHaveHistoryRecord("[PAWN_titleIndef] named [PAWN] was crashed in a nearby transport pod. [He] survived the impact but was badly wounded.", HistoryRecordDefOf.QuestPawnArrived);
    }
    
    // quest.PawnsArrive()
    [RequiresRoyalty]
    public void TestHospitalityRefugee(TestScenario scenario)
    {
        var quest = scenario.Quest(QuestScriptDefOf.Hospitality_Refugee).Execute();
        var pawns = QuestHelper.GetArrivalPawns(quest);
        
        Expect.ThatAll(pawns).ToHaveHistoryRecord("[PAWN][AndOthers] arrived at [PlayerSettlement] seeking shelter and a place to regroup, with nowhere else to go.", HistoryRecordDefOf.QuestPawnArrived);
        Expect.ThatAll(pawns).ToHaveHistoryRecordQuest(quest, HistoryRecordDefOf.QuestPawnArrived);
    }

    // quest.DropPods(), QuestPart_PawnsArrive (raid)
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
            var pawns = QuestHelper.GetArrivalPawns().Where(p => !p.HostileTo(Faction.OfPlayer));
            Expect.ThatAll(pawns).ToHaveHistoryRecord("[PAWN] along with [n] others made an emergency landing at [PlayerSettlement] after their shuttle was damaged.", HistoryRecordDefOf.QuestPawnArrived);
            
            scenario.ForwardTicks(10_000); // QuestNode_Root_ShuttleCrash_Rescue.RaidDelay
        });
        
        scenario.WaitUntil(() => GenHostility.AnyHostileActiveThreatToPlayer(Find.CurrentMap), () =>
        {
            var raiders = QuestHelper.GetArrivalPawns().Where(p => p.HostileTo(Faction.OfPlayer));
            Expect.ThatAll(raiders).ToHaveHistoryRecord("[PAWN] and [n] others from [Faction] attacked the crashed shuttle site.", HistoryRecordDefOf.QuestPawnArrived);
        });
        return () => scenario.SlowDown();
    }
    
    // QuestNode_PawnsArrive, QuestNode_Raid (Raid Incident)
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
    
    // QuestNode_PawnsArrive, QuestNode_Raid
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
    
    // QuestNode_GiveRewards > Reward_Pawn > QuestPart_GiveToCaravan
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
