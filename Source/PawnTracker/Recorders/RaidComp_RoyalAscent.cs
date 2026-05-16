using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RaidComp_RoyalAscent : RaidComp
{
    public override bool Match(BuildInput input) => ModsConfig.RoyaltyActive && input.Quest.root == Extra.QuestScriptDefOf.EndGame_RoyalAscent;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var asker = input.QuestAsker;
        return builder.AddRule("QuestAskerTitle", asker.royalty.MainTitle())
            .AddRule("EmpireFaction", asker.HomeFaction);
    }

    [RequiresRoyalty]
    public void Test(TestScenario scenario)
    {
        var quest = scenario.Quest(Extra.QuestScriptDefOf.EndGame_RoyalAscent).Execute();
        var raiders = scenario.Incident(IncidentDefOf.RaidEnemy).CreateIntervalIncident(i => i.parms.quest == quest).Execute();

        Expect.ThatAll(raiders).ToHaveHistoryRecord(
            HistoryRecordDefOf.Raid,
            $"{GenericRaidDescription}. [He] came to assassinate [QuestAsker], the Stellarch of [EmpireFaction].");
    }
}
