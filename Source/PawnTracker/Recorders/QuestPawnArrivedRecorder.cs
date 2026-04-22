using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using PawnHistory.Source.Helper;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        var involvedFactions = e.Quest.InvolvedFactions.ToList();
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddRule("Quest", e.Quest.name.Colorize(ColoredText.GeneColor))
            .AddRule("Asker", asker)
            .AddRule("Faction", involvedFactions[0])
            .AddRule("Faction2", involvedFactions.Count > 1 ? involvedFactions[1] : null)
            .WithPlayerSettlement(e.Pawn.MapHeld.Parent)
            .WithOthers(e.Group)
            .AddConstant("isEnemy", e.Pawn.Faction.HostileTo(Faction.OfPlayer))
            .AddConstant("quest", HasCustomDescription(questScriptDef, recordDef) ? questScriptDef : string.Empty)
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc, e.Group, quest: e.Quest);
    }

    private static bool HasCustomDescription(string questScriptDef, HistoryRecordDef recordDef)
    {
        return !questScriptDef.NullOrEmpty() && recordDef.descriptionMaker.RulesPlusIncludes.Any(rule =>
            rule.keyword == "entry"
            && Mathf.Approximately(rule.Priority, 1f)
            && rule.constantConstraints != null
            && rule.constantConstraints.Any(constraint => constraint.key == "quest" && constraint.value == questScriptDef)
        );
    }

    private static List<Pawn> GetArrivalPawns(Quest quest = null)
    {
        quest ??= Find.QuestManager.QuestsListForReading.Last();
        var source1 = quest.PartsListForReading.OfType<QuestPart_PawnsArrive>().SelectMany(part => part.pawns);
        var source2 = quest.PartsListForReading.OfType<QuestPart_DropPods>().SelectMany(part => part.Things).OfType<Pawn>();
        return source1.Concat(source2).Where(p => p.MapHeld == Find.CurrentMap).ToList();
    }
    
    public void TestWandererJoin(TestScenario scenario)
    {
        // IncidentWorker_GiveQuest
        scenario.Incident(DefLookup.Incident.WandererJoin).Execute();
        var letter = Find.LetterStack.LettersListForReading.OfType<ChoiceLetter_AcceptJoiner>().Last();
        letter.Choices.First().action(); // accept
        
        var pawns = GetArrivalPawns();
        Expect.That(pawns.Last()).ToHaveHistoryRecord("A [PAWN_title] named [PAWN] arrived and joined the colony. [He] is willing to contribute, but refuses to leave voluntarily, claiming to have nowhere else to go.", HistoryRecordDefOf.QuestPawnArrived);
    }
    
    public void TestRefugeePodCrash(TestScenario scenario)
    {
        // IncidentWorker_GiveQuest
        scenario.Incident(DefLookup.Incident.RefugeePodCrash).Execute();
        var pawns = GetArrivalPawns();
        Expect.That(pawns.Last()).ToHaveHistoryRecord("[PAWN_titleIndef] named [PAWN] was crashed in a nearby transport pod. [He] survived the impact but was badly wounded.", HistoryRecordDefOf.QuestPawnArrived);
    }
    
    public void TestRaidJoiner(TestScenario scenario)
    {
        scenario.Quest(DefLookup.QuestScript.ThreatReward_Raid_Joiner).Execute();
        scenario.ForwardTime(1f);
        
        Expect.Assertions(3);

        GameEventBus.SubscribeOnce<RaidStartedEvent>(e =>
        {
            var pawn = GetArrivalPawns().First(p => p.Faction.IsPlayer);
            
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
            var pawn = GetArrivalPawns().First(p => p.Faction.IsPlayer);
            
            Expect.That(pawn).ToHaveHistoryRecord("[PAWN] joined [PlayerSettlement] after deserting [Faction], while being hunted by a loyalty squad.", HistoryRecordDefOf.QuestPawnArrived);
            Expect.ThatAll(e.Pawns).ToHaveHistoryRecord("[RaidText]. [He] came to hunt down [QuestAsker].", HistoryRecordDefOf.Raid);
            Expect.ThatAll(e.Pawns).ToHaveHistoryRecordConcern(pawn, HistoryRecordDefOf.Raid);
        });
    }
    
    [RequiresRoyalty]
    public void TestHospitalityRefugee(TestScenario scenario)
    {
        scenario.Quest(QuestScriptDefOf.Hospitality_Refugee).Execute();
        var pawns = GetArrivalPawns();
        
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
            var pawns = GetArrivalPawns();
            Expect.ThatAll(pawns).ToHaveHistoryRecord("[PAWN] along with [n] others made an emergency landing at [PlayerSettlement] in a damaged shuttle.", HistoryRecordDefOf.QuestPawnArrived);
            
            scenario.ForwardTime(1f);
        });

        scenario.WaitUntil(() => GetArrivalPawns().Any(p => p.Faction.HostileTo(Faction.OfPlayer)), () =>
        {
            var raiders = GetArrivalPawns().Where(p => p.Faction.HostileTo(Faction.OfPlayer));
            Expect.ThatAll(raiders).ToHaveHistoryRecord("[PAWN] and [n] others from [Faction] attacked the crashed shuttle site.", HistoryRecordDefOf.QuestPawnArrived);
        });
        return () => scenario.SlowDown();
    }
}
