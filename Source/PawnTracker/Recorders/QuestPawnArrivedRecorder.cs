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
    private const string GenericArrivalText = "[PAWN][AlongWithOthers] arrived at [PlayerSettlement] as part of [Quest] quest.";
    private const string GenericRewardText = "[PAWN] arrived at [PlayerSettlement] as a reward for completing [Quest] quest.";
    
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

        Expect.ThatAll(pawns).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.QuestPawnArrived,
            Description = description,
            Quest = quest,
        });
    }
    
    // IncidentWorker_GiveQuest, QuestPart_PawnsArrive, quest.DropPods()
    public void TestIncident(TestScenario scenario)
    {
        scenario.Incident(Extra.IncidentDefOf.WandererJoin).Execute();
        scenario.Letter<ChoiceLetter_AcceptJoiner>().Accept();
        
        var pawns1 = QuestHelper.GetArrivalPawns();
        Expect.That(pawns1.Last()).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.QuestPawnArrived,
            Description = "A [PAWN_title] named [PAWN] arrived and joined the colony. [He] is willing to contribute, but refuses to leave voluntarily, claiming to have nowhere else to go.",
        });
        
        scenario.Incident(Extra.IncidentDefOf.RefugeePodCrash).Execute();
        var pawns2 = QuestHelper.GetArrivalPawns();
        Expect.That(pawns2.Last()).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.QuestPawnArrived,
            Description = "[PAWN_titleIndef] named [PAWN] was crashed in a nearby transport pod. [He] survived the impact but was badly wounded.",
        });
    }
    
    // quest.PawnsArrive()
    [RequiresRoyalty]
    public void TestHospitalityRefugee(TestScenario scenario)
    {
        var quest = scenario.Quest(QuestScriptDefOf.Hospitality_Refugee).Execute();
        var pawns = QuestHelper.GetArrivalPawns(quest);
        
        Expect.ThatAll(pawns).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.QuestPawnArrived,
            Description = "[PAWN][AndOthers] arrived at [PlayerSettlement] seeking shelter and a place to regroup, with nowhere else to go.",
            Quest = quest,
        });
    }

    // quest.DropPods(), QuestPart_PawnsArrive (raid)
    [RequiresRoyalty]
    public Action TestShuttleCrashRescue(TestScenario scenario)
    {
        TestManager.Timeout = 100000000;
        scenario.SpeedUp();
        Expect.Assertions(2);
        
        var quest = scenario.Quest(Extra.QuestScriptDefOf.ShuttleCrash_Rescue).Execute();

        // QuestNode_Root_ShuttleCrash_Rescue.QuestStartDelay
        TickDelayManager.Delay(120, () =>
        {
            var pawns = QuestHelper.GetArrivalPawns().Where(p => !p.HostileTo(Faction.OfPlayer));
            Expect.ThatAll(pawns).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.QuestPawnArrived,
                Description = "[PAWN] along with [n] others made an emergency landing at [PlayerSettlement] after their shuttle was damaged.",
                Quest = quest,
            });
            
            scenario.ForwardTicks(10_000); // QuestNode_Root_ShuttleCrash_Rescue.RaidDelay
        });
        
        scenario.WaitUntil(() => GenHostility.AnyHostileActiveThreatToPlayer(Find.CurrentMap), () =>
        {
            var raiders = QuestHelper.GetArrivalPawns().Where(p => p.HostileTo(Faction.OfPlayer));
            Expect.ThatAll(raiders).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.QuestPawnArrived,
                Description = "[PAWN] and [n] others from [Faction] attacked the crashed shuttle site.",
                Quest = quest,
            });
        });
        return () => scenario.SlowDown();
    }
    
    // QuestNode_PawnsArrive, QuestNode_Raid (Raid Incident)
    public void TestRaidJoiner(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.ThreatReward_Raid_Joiner).Execute();
        scenario.ForwardDays(1f);
        
        Expect.Assertions(2);

        GameEventBus.SubscribeOnce<RaidStartedEvent>(e =>
        {
            var pawn = QuestHelper.GetArrivalPawns().First(p => p.Faction.IsPlayer);
            
            Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.QuestPawnArrived,
                Description = "[PAWN] joined the colony in exchange for safety while being pursued.",
                Quest = quest,
            });
            Expect.ThatAll(e.Pawns).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.Raid,
                Description = "[RaidText]. [He] came to take [QuestAsker].",
                Concerns = [pawn],
                Quest = e.Quest,
            });
        });
    }
    
    // QuestNode_PawnsArrive, QuestNode_Raid
    [RequiresRoyalty]
    public void TestEmpireDeserter(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.Intro_Deserter).Execute();
        scenario.ForwardDays(1f);
        
        Expect.Assertions(2);

        GameEventBus.SubscribeOnce<RaidStartedEvent>(e =>
        {
            var pawn = QuestHelper.GetArrivalPawns().First(p => p.Faction.IsPlayer);
            
            Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.QuestPawnArrived,
                Description = "[PAWN] joined [PlayerSettlement] after deserting [Faction], while being hunted by a loyalty squad.",
                Quest = quest,
            });
            Expect.ThatAll(e.Pawns).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.Raid,
                Description = "[RaidText]. [He] came to hunt down [QuestAsker].",
                Concerns = [pawn],
                Quest = e.Quest,
            });
        });
    }

    public static (Quest, Pawn) SetupQuestWithReward(TestScenario scenario, QuestScriptDef questScriptDef)
    {
        var rewardPawn = scenario.Pawn().WorldPawn().CreateSingle(false);
        
        scenario.ForceRewardPawnInQuest = rewardPawn;

        var quest = scenario.Quest(questScriptDef)
            .ChooseReward(choice => choice.rewards.OfType<Reward_Pawn>().Any())
            .Execute();

        return (quest, rewardPawn);
    }
    
    // QuestNode_GiveRewards > Reward_Pawn > QuestPart_GiveToCaravan
    public void TestTradeRequest(TestScenario scenario)
    {
        var colonist = scenario.Pawn().Colonist().CreateSingle();
        var (quest, rewardPawn) = SetupQuestWithReward(scenario, Extra.QuestScriptDefOf.TradeRequest);
        var tradePart = quest.GetFirstPartOfType<QuestPart_InitiateTradeRequest>();
        var gifts = scenario.Thing(tradePart.requestedThingDef).Stack(tradePart.requestedCount).Create();

        scenario.Caravan([colonist]).Give(gifts).FulfillTradeRequest(tradePart.settlement).Execute();
        
        Expect.That(rewardPawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.QuestPawnArrived,
            Description = "[PAWN] joined the colony as a reward for fulfilling a trade request from [Faction].",
            Quest = quest,
        });
    }
    
    [SkipTest]
    [RequiresRoyalty]
    public void TestRoyalty(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.WandererJoinAbasia).Execute();
        AssertArrived(quest, GenericArrivalText);
    }

    [SkipTest]
    [RequiresRoyalty]
    public void TestHospitalityJoiners(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.Hospitality_Joiners).Execute();
        AssertArrived(quest, GenericArrivalText);
    }

    [SkipTest]
    [RequiresRoyalty]
    public void TestHospitalityPrisoners(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.Hospitality_Prisoners).Execute();
        AssertArrived(quest, GenericArrivalText);
    }

    [SkipTest]
    [RequiresRoyalty]
    public void TestThreatRewardInfestationJoiner(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.ThreatReward_Infestation_Joiner).Execute();
        AssertArrived(quest, GenericArrivalText, pawn => pawn.Faction?.IsPlayer == true);
    }

    [SkipTest]
    [RequiresRoyalty]
    public void TestThreatRewardManhuntersJoiner(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.ThreatReward_Manhunters_Joiner).Execute();
        AssertArrived(quest, GenericArrivalText, pawn => pawn.Faction?.IsPlayer == true);
    }

    [SkipTest]
    [RequiresRoyalty]
    public void TestThreatRewardGameConditionJoiner(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.ThreatReward_GameCondition_Joiner).Execute();
        AssertArrived(quest, GenericArrivalText, pawn => pawn.Faction?.IsPlayer == true);
    }

    [SkipTest]
    [RequiresRoyalty]
    public void TestThreatRewardSiteThreatJoiner(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.ThreatReward_SiteThreat_Joiner).Execute();
        AssertArrived(quest, GenericArrivalText, pawn => pawn.Faction?.IsPlayer == true);
    }

    [SkipTest]
    [RequiresRoyalty]
    public void TestThreatRewardRaidMultiFactionJoiner(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.ThreatReward_RaidMultiFaction_Joiner).Execute();
        AssertArrived(quest, GenericArrivalText, pawn => pawn.Faction?.IsPlayer == true);
    }

    [SkipTest]
    [RequiresRoyalty]
    public void TestThreatRewardMysteryThreatJoiner(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.ThreatReward_MysteryThreat_Joiner).Execute();
        AssertArrived(quest, GenericArrivalText, pawn => pawn.Faction?.IsPlayer == true);
    }

    [SkipTest]
    [RequiresRoyalty]
    public Action TestHospitalityPawnReward(TestScenario scenario)
    {
        var rewardPawn = scenario.Pawn().WorldPawn().CreateSingle(false);
        
        scenario.ForceRewardPawnInQuest = rewardPawn;
        var quest = scenario.Quest(Extra.QuestScriptDefOf.Hospitality_Joiners).Execute();

        Expect.Assertions(1);
        scenario.SpeedUp();
        scenario.RunUntil(
            () => rewardPawn.HistoryRecords.Any(record => record.def == HistoryRecordDefOf.QuestPawnArrived),
            () => scenario.ForwardDays(0.25f),
            () =>
            {
                Expect.That(rewardPawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
                {
                    Def = HistoryRecordDefOf.QuestPawnArrived,
                    Description = GenericRewardText,
                    Quest = quest,
                });
            },
            60);

        return () => scenario.SlowDown();
    }
}
