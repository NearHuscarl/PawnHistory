using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class QuestPawnArrivedComp_RaidMiscReward : QuestPawnArrivedComp
{
    public override bool Match(Quest quest) => quest.root.defName == nameof(Extra.QuestScriptDefOf.ThreatReward_Raid_MiscReward);

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var hostileFaction = input.Quest.InvolvedFactions.First(f => f.HostileTo(Faction.OfPlayer));
        return builder.AddRule("HostileFaction", hostileFaction);
    }

    [RequiresRoyalty]
    public void Test(TestScenario scenario)
    {
        TestManager.Timeout = 999999999;
        var map = Find.CurrentMap;
        var (quest, rewardPawn) = QuestPawnArrivedRecorder.SetupQuestWithReward(scenario, Extra.QuestScriptDefOf.ThreatReward_Raid_MiscReward, 5);
        
        scenario.SpeedUp();
        scenario.ForwardTicks(300000); // firstRaidDelayTicks
        scenario.WaitUntil(() => GenHostility.AnyHostileActiveThreatToPlayer(map), () =>
        {
            map.mapPawns.AllHumanlikeSpawned.Where(p => p.HostileTo(Faction.OfPlayer)).ToList().ForEach(p => p.Kill(null));
        });

        Expect.That(rewardPawn).Eventually().ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.QuestPawnArrived,
            Description = "[PAWN] joined the colony as a reward for defeating a raid from [HostileFaction].",
            Quest = quest,
        });
    }

    [RequiresRoyalty]
    public void TestHelper(TestScenario scenario)
    {
        scenario.AlwaysHaveHelpersInQuest = true;
        scenario.SpeedUp();
        
        var quest = scenario.Quest(Extra.QuestScriptDefOf.ThreatReward_Raid_MiscReward).Execute();
        var helpers = QuestHelper.GetQuestPawns(quest).Where(p => QuestHelper.GetQuestPawnKind(quest, p) == QuestPawnKind.Helper);

        Expect.ThatAll(helpers).Eventually().ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.QuestPawnArrived,
            Description = "[PAWN] along with [n] others arrived at the colony to help fight off a raid from [HostileFaction].",
            Quest = quest,
        });
    }
}
