using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RaidComp_WorkSite : RaidComp
{
    public override bool Match(BuildInput input) => ModsConfig.IdeologyActive && input.Quest?.root == Extra.QuestScriptDefOf.OpportunitySite_WorkSite;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder.AddRule("pawnsPlural", input.Faction.def.pawnsPlural);
    }
    
    [RequiresIdeology]
    public void Test(TestScenario scenario)
    {
        scenario.SpeedUp();

        var quest = scenario.Quest(Extra.QuestScriptDefOf.OpportunitySite_WorkSite, 2000).Execute();
        var site = QuestHelper.GetWorldObject<Site>(quest);
        var pawns = scenario.Pawn().Colonist().Armed().Execute();

        quest.GetFirstPartOfType<QuestPart_SurpriseReinforcement>().reinforcementChance = 1f;
        
        Expect.Assertions(1);

        scenario.Caravan(pawns)
            .VisitSite(site)
            // trigger QuestPart_SurpriseReinforcement
            .OnMapGenerated(e => e.Map.mapPawns.AllPawnsSpawned.First(p => p.HostileTo(Faction.OfPlayer)).Kill(null))
            .Execute();

        scenario.RunOnceOn<RaidStartedEvent>(e => Expect.ThatAll(e.Pawns).ToHaveHistoryRecord(
            HistoryRecordDefOf.Raid, 
            $"{GenericRaidDescription}. [He] moved in to help [His] [People], who were under attack."));
    }
}
