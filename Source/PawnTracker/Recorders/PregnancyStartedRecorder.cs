using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PregnancyStartedRecorder : RecorderBase<PregnancyStartedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<PregnancyStartedEvent>(CreateRecord);
    }

    public override void CreateRecord(PregnancyStartedEvent e)
    {
        List<Thing> concerns = [e.Father, e.Mother, e.Carrier];

        if (ShouldRecord(e.Carrier))
            AddRecord(HistoryRecordDefOf.PregnancyStarted, e.Carrier, CreateDescription(e), concerns);

        if (ShouldRecord(e.Mother) && e.Mother != e.Carrier)
            AddRecord(HistoryRecordDefOf.PregnancyStarted, e.Mother, CreateDescription(e), concerns);

        if (ShouldRecord(e.Father))
            AddRecord(HistoryRecordDefOf.PregnancyStarted, e.Father, CreateDescription(e, "Father"), concerns);
    }

    private static TaggedString CreateDescription(PregnancyStartedEvent e, string pov = null)
    {
        var relation = e.Father?.GetMostImportantRelation(e.Carrier)?.GetGenderSpecificLabel(e.Carrier);
        var isIvf = e.Mother != null && e.Carrier != e.Mother;

        return HistoryRecordDefOf.PregnancyStarted.Description(e.Carrier, "Carrier")
            .AddRule("Father", e.Father)
            .AddRule("Mother", e.Mother)
            .AddRule("Relation", relation)
            .AddConstant("pov", pov)
            .AddConstant("hasFather", e.Father != null)
            .AddConstant("hasRelation", relation != null)
            .AddConstant("isIvf", isIvf)
            .Resolve();
    }

    [RequiresBiotech]
    public void Test(TestScenario scenario)
    {
        var father = scenario.Pawn().Colonist().CreateSingle();
        var carrier = scenario.Pawn()
            .Colonist()
            .SetRelation(father, PawnRelationDefOf.Spouse)
            .CreateSingle();

        AddPregnancy(scenario, carrier, null, father);

        Expect.That(carrier).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.PregnancyStarted,
            Description = "[PAWN] became pregnant with [Father]'s child.",
            Concerns = [father]
        });
        Expect.That(father).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.PregnancyStarted,
            Description = "[PAWN]'s wife, [Carrier], became pregnant.",
            Concerns = [carrier]
        });
    }

    [RequiresBiotech]
    public void TestIvf(TestScenario scenario)
    {
        var father = scenario.Pawn().Colonist().CreateSingle();
        var mother = scenario.Pawn().Colonist().CreateSingle();
        var carrier = scenario.Pawn().Colonist().CreateSingle();

        AddPregnancy(scenario, carrier, mother, father);

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.PregnancyStarted,
            Description = "[Carrier] became pregnant through embryo implantation. The baby had the genetic makeup of [Father] and [Mother].",
            Concerns = [father]
        };
        Expect.That(carrier).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [father, mother] }));
        Expect.That(mother).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [carrier, father] }));
        Expect.That(father).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [carrier, mother] }));
    }

    [RequiresBiotech]
    public void TestNoFather(TestScenario scenario)
    {
        var carrier = scenario.Pawn().Colonist().CreateSingle();

        AddPregnancy(scenario, carrier, null, null);

        Expect.That(carrier).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.PregnancyStarted,
            Description = "[PAWN] became pregnant."
        });
    }

    private static void AddPregnancy(TestScenario scenario, Pawn carrier, Pawn mother, Pawn father)
    {
        Hediff_Pregnant hediff = null; 
        if (father != null)
            scenario.Pawn(father).Colonist().SetGender(Gender.Male).CreateSingle();
        if (mother != null)
            scenario.Pawn(mother).Colonist().SetGender(Gender.Female).CreateSingle();
        if (carrier != null)
            scenario.Pawn(carrier).Colonist().SetGender(Gender.Female)
                .AddHediff(HediffDefOf.PregnantHuman, hediffCreated: h => hediff = h as Hediff_Pregnant)
                .CreateSingle();

        hediff!.SetParents(mother, father, null);
        Accessor.HediffComp_MessageAfterTicks.TicksUntilMessage(hediff.GetComp<HediffComp_MessageAfterTicks>()) = 0;
    }
}
