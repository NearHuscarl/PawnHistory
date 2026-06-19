using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RitualOutcomeComp_AnimaTreeLinking : RitualOutcomeComp
{
    public override bool Match(BuildInput input) => input.Event.RitualDef == PreceptDefOf.AnimaTreeLinking;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder.AddRule("Tree", GetTree(input).def, addSubsymbols: true);
    }

    public override IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        yield return GetTree(input);
    }

    private static Thing GetTree(BuildInput input)
    {
        return input.Event.Targets.First(t => t.def == ThingDefOf.Plant_TreeAnima);
    }

    [RequiresIdeology]
    [RequiresRoyalty]
    public void Test(TestScenario scenario)
    {
        scenario.SpeedUp();

        var tribalIdeo = scenario.Ideo().AddPrecept(PreceptDefOf.AnimaTreeLinking).Execute();
        var organizer = scenario.Pawn()
            .Colonist()
            .SetIdeo(tribalIdeo)
            .SetNaturalMeditation()
            .CreateSingle();
        var spectators = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(tribalIdeo)
            .SetNaturalMeditation()
            .Execute();

        var animaTree = scenario.Thing(ThingDefOf.Plant_TreeAnima).CreateSingle();
        var subplantComp = animaTree.TryGetComp<CompSpawnSubplant>();
        for (var i = 0; i < 20; i++)
            subplantComp.AddProgress(1f, ignoreMultiplier: true); // anima tree can be linked once it grows 20 grass

        scenario
            .Ritual(organizer)
            .AnimaTreeLinking(animaTree, spectators)
            .Execute();

        Expect.That(organizer).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RitualOutcome,
            Description = "[PAWN] linked with an anima tree in front of 2 others.",
            Concerns = [animaTree],
        });
        Expect.That(organizer).ToHaveTheLastHistoryRecordsOf([HistoryRecordDefOf.RitualOutcome, HistoryRecordDefOf.PsylinkLevelGained]);
    }
}
