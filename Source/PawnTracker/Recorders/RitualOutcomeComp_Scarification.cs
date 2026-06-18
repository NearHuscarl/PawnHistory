using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RitualOutcomeComp_Scarification : RitualOutcomeComp
{
    public override bool Match(BuildInput input) => input.Event.RitualDef == Extra.PreceptDefOf.ScarificationCeremony;

    public override IEnumerable<Pawn> GetRecordPawns(BuildInput input)
    {
        yield return GetDoer(input);
        yield return GetTarget(input);
    }

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder
            .AddRule("Doer", GetDoer(input))
            .AddRule("Target", GetTarget(input));
    }

    public override IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        yield return GetDoer(input);
        yield return GetTarget(input);
    }

    private static Pawn GetDoer(BuildInput input) => input.Event.AssignedRoles[RitualRoleId.Doer].First();
    private static Pawn GetTarget(BuildInput input) => input.Event.AssignedRoles[RitualRoleId.Target].First();

    [RequiresIdeology]
    public void Test(TestScenario scenario)
    {
        scenario.SpeedUp();

        var scarificationIdeo = scenario.Ideo()
            .AddPrecept(Extra.PreceptDefOf.Scarification_Minor)
            .AddPrecept(Extra.PreceptDefOf.ScarificationCeremony)
            .Execute();
        var doer = scenario.Pawn()
            .Colonist()
            .SetIdeo(scarificationIdeo, role: PreceptDefOf.IdeoRole_Moralist)
            .CreateSingle();
        var target = scenario.Pawn()
            .Colonist()
            .SetIdeo(scarificationIdeo)
            .CreateSingle();
        var spectators = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(scarificationIdeo)
            .Execute();
        var outcome = Extra.RitualOutcomeEffectDefOf.ScarificationCeremony.WorstOutcome;

        scenario.Map()
            .BuildRoom(8, 8, floorDef: TerrainDefOf.MetalTile)
            .AsShrine(scarificationIdeo)
            .Execute();

        scenario
            .Ritual(doer)
            .Outcome(outcome)
            .ScarificationCeremony(target, spectators)
            .Execute();

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RitualOutcome,
            Description = "[Target] was scarified by [Doer] during a terrible [Ritual] in front of 2 others.",
        };
        Expect.That(doer).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [target] }));
        Expect.That(target).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [doer] }));
    }
}
