using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RitualOutcomeComp_TreeConnection : RitualOutcomeComp
{
    public override bool Match(BuildInput input) => input.Event.RitualDef == Extra.PreceptDefOf.TreeConnection;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder.AddRule("Tree", GetTree(input).def, addSubsymbols: true);
    }

    public override IEnumerable<Pawn> GetRecordPawns(BuildInput input)
    {
        var connector = GetConnector(input);
        var treeConnection = GetTree(input).TryGetComp<CompTreeConnection>();
        if (treeConnection?.ConnectedPawn != connector)
            return [];

        return [connector];
    }

    public override IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        yield return GetTree(input);
    }

    private static Pawn GetConnector(BuildInput input)
    {
        return input.Event.Host;
    }

    private static Thing GetTree(BuildInput input)
    {
        return input.Event.Targets.First(t => t.def == ThingDefOf.Plant_TreeGauranlen);
    }

    [RequiresIdeology]
    public void Test(TestScenario scenario)
    {
        scenario.SpeedUp();

        var treeConnectionIdeo = scenario.Ideo().AddPrecept(Extra.PreceptDefOf.TreeConnection).Execute();
        var connector = scenario.Pawn()
            .Colonist()
            .SetIdeo(treeConnectionIdeo)
            .CreateSingle();
        var spectators = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(treeConnectionIdeo)
            .Execute();
        var gauranlenTree = scenario.Thing(ThingDefOf.Plant_TreeGauranlen).CreateSingle();

        scenario
            .Ritual(connector)
            .TreeConnection(gauranlenTree, spectators)
            .Execute();

        Expect.That(connector).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RitualOutcome,
            Description = "[PAWN] connected with a Gauranlen tree in front of 2 others.",
            Concerns = [gauranlenTree],
        });
    }
}
