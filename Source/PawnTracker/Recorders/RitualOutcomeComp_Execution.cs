using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RitualOutcomeComp_Execution : RitualOutcomeComp
{
    public override bool Match(BuildInput input) => input.Event.RitualDef == Extra.PreceptDefOf.Execution;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var executedPawn = input.Event.AssignedRoles[RitualRoleId.Prisoner].First();

        return builder.AddRule("ExecutedPawn", executedPawn, addSubsymbols: true);
    }

    public override IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        yield return input.Event.AssignedRoles[RitualRoleId.Prisoner].First();
    }

    [RequiresIdeology]
    public void Test(TestScenario scenario)
    {
        scenario.SpeedUp();

        var executionIdeo = scenario.Ideo().AddPrecept(Extra.PreceptDefOf.Execution).Execute();
        var organizer = scenario.Pawn()
            .Colonist()
            .SetIdeo(executionIdeo)
            .CreateSingle();
        var spectators = scenario.Pawn(2).Colonist().Execute();
        var prisoners = new List<Pawn>();

        scenario.Map()
            .BuildRoom(8, 8, "prison")
            .AsPrison(1, prisoners: prisoners)
            .Execute();

        scenario.Map()
            .BuildRoom(MapBuilder.Beside("prison", Rot4.East, 8, 8), "shrine", floorDef: TerrainDefOf.MetalTile)
            .AsShrine(executionIdeo)
            .Execute();

        scenario
            .Ritual(organizer)
            .Outcome(Extra.RitualOutcomeEffectDefOf.Execution.BestOutcome)
            .Execution(prisoners[0], spectators)
            .Execute();

        Expect.That(organizer).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RitualOutcome,
            Description = "[PAWN] carried out a spectacular public execution of [ExecutedPawn] before 2 others.",
            Concerns = [prisoners[0]],
        });
    }
}
