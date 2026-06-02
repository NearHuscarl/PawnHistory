using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RitualOutcomeComp_ChildBirth : RitualOutcomeComp
{
    public override bool Match(BuildInput input) => input.Event.RitualDef == PreceptDefOf.ChildBirth;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder
            .AddRule("Carrier", GetCarrier(input), addSubsymbols: true)
            .AddConstant("outcome", input.Event.OutcomeLabel);
    }

    public override IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        yield return GetCarrier(input);
    }

    private static Pawn GetCarrier(BuildInput input)
    {
        return input.Event.AssignedRoles[RitualRoleId.Mother].First();
    }

    private static (Pawn doctor, Pawn carrier) SetupChildBirthRitual(TestScenario scenario, List<Pawn> spectators, RitualOutcomePossibility outcome)
    {
        scenario.SpeedUp();

        var childbirthIdeo = scenario.Ideo().AddPrecept(PreceptDefOf.ChildBirth).Execute();
        scenario.Map()
            .BuildRoom(7, 7, "Hospital")
            .AsHospital(1)
            .Execute();

        var doctor = scenario.Pawn()
            .Colonist()
            .SetIdeo(childbirthIdeo)
            .SetDoctor()
            .CreateSingle();
        var carrier = scenario.Pawn()
            .Colonist()
            .SetGender(Gender.Female)
            .SetIdeo(childbirthIdeo)
            .AddHediff(HediffDefOf.PregnancyLabor) // required in RitualRole_Mother
            .CreateSingle();

        scenario
            .Ritual(doctor)
            .Outcome(outcome)
            .ChildBirth(carrier, spectators)
            .Execute();

        return (doctor, carrier);
    }

    [RequiresBiotech]
    [RequiresIdeology]
    public void Test(TestScenario scenario)
    {
        var (doctor, carrier) = SetupChildBirthRitual(scenario, [], RitualOutcomeEffectDefOf.ChildBirth.WorstOutcome);

        Expect.That(doctor).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RitualOutcome,
            Description = "[Host] delivered a stillborn baby for [Carrier].",
            Concerns = [carrier],
        });
        Expect.That(carrier).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.RitualOutcome);
    }

    [RequiresBiotech]
    [RequiresIdeology]
    public void TestWithSpectator(TestScenario scenario)
    {
        var spectators = scenario.Pawn(2).Colonist().Execute();
        var (doctor, carrier) = SetupChildBirthRitual(scenario, spectators, RitualOutcomeEffectDefOf.ChildBirth.BestOutcome);

        Expect.That(doctor).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RitualOutcome,
            Description = "[Host] delivered a healthy baby for [Carrier] in front of 2 others.",
            Concerns = [carrier],
        });
        Expect.That(carrier).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.RitualOutcome);
    }
}
