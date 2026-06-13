using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RitualOutcomeComp_Trial : RitualOutcomeComp
{
    public override bool Match(BuildInput input)
    {
        return input.Event.RitualDef == Extra.PreceptDefOf.Trial
            || input.Event.RitualDef == Extra.PreceptDefOf.TrialPrisoner
            || input.Event.RitualDef == Extra.PreceptDefOf.TrialMentalState;
    }

    public override IEnumerable<Pawn> GetRecordPawns(BuildInput input)
    {
        yield return GetJudge(input);
        yield return GetConvict(input);
    }

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder
            .AddRule("Judge", GetJudge(input))
            .AddRule("Convict", GetConvict(input));
    }

    public override IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        yield return GetJudge(input);
        yield return GetConvict(input);
    }

    private static Pawn GetJudge(BuildInput input) => input.Event.AssignedRoles.GetValueOrDefault(RitualRoleId.Leader).First();
    private static Pawn GetConvict(BuildInput input) => input.Event.AssignedRoles.GetValueOrDefault(RitualRoleId.Convict).First();

    private static Pawn SetupJudge(TestScenario scenario, Ideo trialIdeo)
    {
        return scenario.Pawn()
            .Colonist()
            .SetIdeo(trialIdeo, role: PreceptDefOf.IdeoRole_Leader)
            .CreateSingle();
    }

    private static void SetupShrine(TestScenario scenario, Ideo trialIdeo)
    {
        scenario.Map()
            .BuildRoom(10, 10, floorDef: TerrainDefOf.MetalTile)
            .AsShrine(trialIdeo)
            .WithThing(ThingDefOf.Beer, 30) // mental state
            .Execute();
    }

    private static void AssertTrialRecords(Pawn judge, Pawn convict, List<Pawn> spectators, RitualOutcomePossibility outcome)
    {
        var description = outcome == Extra.RitualOutcomeEffectDefOf.Trial.WorstOutcome
            ? "[Convict] was exonerated in [Judge]'s trial before 2 others."
            : "[Convict] was convicted in [Judge]'s trial before 2 others.";

        Expect.That(judge).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RitualOutcome,
            Description = description,
            Concerns = [convict],
        });
        Expect.That(convict).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RitualOutcome,
            Description = description,
            Concerns = [judge],
        });
    }

    [RequiresIdeology]
    public void Test(TestScenario scenario)
    {
        scenario.SpeedUp();

        var trialIdeo = scenario.Ideo().AddPrecept(Extra.PreceptDefOf.Trial).Execute();
        var judge = SetupJudge(scenario, trialIdeo);
        var convict = scenario.Pawn()
            .Colonist()
            .SetIdeo(trialIdeo)
            .CreateSingle();
        var spectators = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(trialIdeo)
            .Execute();
        var outcome = Extra.RitualOutcomeEffectDefOf.Trial.WorstOutcome;

        SetupShrine(scenario, trialIdeo);

        scenario
            .Ritual(judge)
            .Outcome(outcome)
            .Trial(convict, spectators)
            .Execute();

        AssertTrialRecords(judge, convict, spectators, outcome);
    }

    [RequiresIdeology]
    public void TestPrisoner(TestScenario scenario)
    {
        scenario.SpeedUp();

        var trialIdeo = scenario.Ideo().AddPrecept(Extra.PreceptDefOf.TrialPrisoner).Execute();
        var judge = SetupJudge(scenario, trialIdeo);
        var spectators = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(trialIdeo)
            .Execute();
        var prisoners = new List<Pawn>();
        var outcome = Extra.RitualOutcomeEffectDefOf.Trial.BestOutcome;

        scenario.Map()
            .BuildRoom(8, 8, "prison")
            .AsPrison(1, prisoners: prisoners)
            .Execute();
        scenario.Pawn(prisoners)
            .SetIdeo(trialIdeo)
            .Execute();

        SetupShrine(scenario, trialIdeo);

        scenario
            .Ritual(judge)
            .Outcome(outcome)
            .Trial(prisoners[0], spectators)
            .Execute();

        AssertTrialRecords(judge, prisoners[0], spectators, outcome);
    }

    [RequiresIdeology]
    public void TestMentalState(TestScenario scenario)
    {
        scenario.SpeedUp();

        var trialIdeo = scenario.Ideo().AddPrecept(Extra.PreceptDefOf.TrialMentalState).Execute();
        var judge = SetupJudge(scenario, trialIdeo);

        SetupShrine(scenario, trialIdeo);

        var convict = scenario.Pawn()
            .Colonist()
            .SetIdeo(trialIdeo)
            .StopMentalState()
            .Do(p => p.StartMentalBreakWithMadeUpThought(Extra.MentalBreakDefOf.Binging_DrugMajor))
            .CreateSingle();
        var spectators = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(trialIdeo)
            .Execute();
        var outcome = Extra.RitualOutcomeEffectDefOf.Trial.BestOutcome;

        Expect.That(convict.InMentalState).True();

        scenario
            .Ritual(judge)
            .Outcome(outcome)
            .Trial(convict, spectators)
            .Execute();

        AssertTrialRecords(judge, convict, spectators, outcome);
    }
}
