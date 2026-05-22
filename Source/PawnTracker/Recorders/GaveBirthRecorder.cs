using System.Linq;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class GaveBirthRecorder : RecorderBase<GaveBirthEvent>
{
    private const string BabyPov = "baby";
    private const string CarrierPov = "carrier";

    public override void Register()
    {
        GameEventBus.Subscribe<GaveBirthEvent>(CreateRecord);
    }

    public override void CreateRecord(GaveBirthEvent e)
    {
        if (e.Baby == null)
            return;

        var recordDef = HistoryRecordDefOf.GaveBirth;
        
        if (ShouldRecord(e.Baby))
            AddRecord(recordDef, e.Baby, CreateDescription(e, BabyPov), [e.Carrier, e.GeneticMother, e.Father]);

        if (ShouldRecord(e.Carrier))
            AddRecord(recordDef, e.Carrier, CreateDescription(e, CarrierPov), [e.Baby, e.GeneticMother, e.Father]);
        
        if (e.IsVatBirth && ShouldRecord(e.GeneticMother))
            AddRecord(recordDef, e.GeneticMother, CreateDescription(e, CarrierPov), [e.Baby, e.Father]);
    }

    private static string CreateDescription(GaveBirthEvent e, string pov)
    {
        return HistoryRecordDefOf.GaveBirth.Description(e.Baby, "Baby")
            .IncludePawnGrammar()
            .AddRule("Carrier", e.Carrier)
            .AddRule("GeneticMother", e.GeneticMother)
            .AddRule("Father", e.Father)
            .AddConstant("pov", pov)
            .AddConstant("outcome", e.OutcomeLabel.ToLowerInvariant())
            .AddConstant("isVatBirth", e.IsVatBirth)
            .AddConstant("isSurrogacy", e.IsSurrogacy)
            .AddConstant("isInbred", e.IsInbred)
            .AddConstant("hasVatParents", e.GeneticMother != null && e.Father != null)
            .Resolve(e.IsVatBirth ? "entryVatBirth" : "entry");
    }

    [RequiresBiotech]
    public void TestNaturalHealthy(TestScenario scenario)
    {
        AssertNaturalBirthOutcome(scenario, 1, "[Carrier] gave birth to a healthy baby!", "[Baby] was born healthy to [Carrier]");
    }

    [RequiresBiotech]
    public void TestNaturalSick(TestScenario scenario)
    {
        AssertNaturalBirthOutcome(scenario, 0, "[Carrier] gave birth to a sick baby.", "[Baby] was born sick to [Carrier].");
    }

    [RequiresBiotech]
    public void TestNaturalStillborn(TestScenario scenario)
    {
        AssertNaturalBirthOutcome(scenario, -1, "[Carrier] gave birth to [Baby].", "[Baby] was born to [Carrier].");
    }

    [RequiresBiotech]
    public void TestNaturalMotherDied(TestScenario scenario)
    {
        scenario.ForceMotherDeathDuringBirth = true;
        AssertNaturalBirthOutcome(scenario, 0, "[Carrier] gave birth to a sick baby.", "[Baby] was born sick to [Carrier].");
    }

    private static void AssertNaturalBirthOutcome(TestScenario scenario, int positivityIndex, string povCarrierDesc, string povBabyDescription)
    {
        var mother = CreateParent(scenario, Gender.Female);
        var father = CreateParent(scenario, Gender.Male);
        var baby = ApplyBirthOutcome(positivityIndex, mother, mother, father);

        Expect.That(baby).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.GaveBirth,
            Description = povBabyDescription,
            Concerns = [mother, father]
        });
        Expect.That(mother).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.GaveBirth,
            Description = povCarrierDesc,
            Concerns = [baby, father]
        });
        
        if (positivityIndex == -1) // stillborn
        {
            Expect.That(baby).ToHaveHistoryRecordOf(HistoryRecordDefOf.Death);
            Expect.That(mother).ToHaveHistoryRecordOf(HistoryRecordDefOf.RelativeDeath);
        }
    }

    [RequiresBiotech]
    public void TestSurrogacyAndInbred(TestScenario scenario)
    {
        scenario.ForceInbred = true;

        var carrier = CreateParent(scenario, Gender.Female);
        var geneticMother = CreateParent(scenario, Gender.Female);
        var father = CreateParent(scenario, Gender.Male);
        var baby = ApplyBirthOutcome(1, geneticMother, carrier, father);

        Expect.That(baby).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.GaveBirth,
            Description = "[Baby] was born healthy to [Carrier]. [He] was conceived through in vitro fertilization, with [GeneticMother] and [Father] as genetic parents. [Baby] showed genetic abnormalities caused by inbreeding.",
            Concerns = [carrier, geneticMother, father]
        });
        Expect.That(carrier).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.GaveBirth,
            Description = "[Carrier] gave birth to a healthy baby! [He] was conceived through in vitro fertilization, with [GeneticMother] and [Father] as genetic parents. [Baby] showed genetic abnormalities caused by inbreeding.",
            Concerns = [baby, geneticMother, father]
        });
    }

    [RequiresBiotech]
    public void TestVatHealthy(TestScenario scenario)
    {
        AssertVatBirthOutcome(scenario, 1, true, "[Baby] was born healthy from a growth-vat embryo of [GeneticMother] and [Father].");
    }

    [RequiresBiotech]
    public void TestVatSick(TestScenario scenario)
    {
        AssertVatBirthOutcome(scenario, 0, true, "[Baby] was born sick from a growth-vat embryo of [GeneticMother] and [Father].");
    }

    [RequiresBiotech]
    public void TestVatStillborn(TestScenario scenario)
    {
        AssertVatBirthOutcome(scenario, -1, true, "[Baby] was born from a growth-vat embryo of [GeneticMother] and [Father].");
    }

    [RequiresBiotech]
    public void TestVatHealthyNoParents(TestScenario scenario)
    {
        AssertVatBirthOutcome(scenario, 1, false, "[Baby] was born healthy from a growth-vat embryo.");
    }

    [RequiresBiotech]
    public void TestVatSickNoParents(TestScenario scenario)
    {
        AssertVatBirthOutcome(scenario, 0, false, "[Baby] was born sick from a growth-vat embryo.");
    }

    [RequiresBiotech]
    public void TestVatStillbornNoParents(TestScenario scenario)
    {
        AssertVatBirthOutcome(scenario, -1, false, "[Baby] was born from a growth-vat embryo.");
    }

    private static void AssertVatBirthOutcome(TestScenario scenario, int positivityIndex, bool hasParents, string expectedDescription)
    {
        var geneticMother = CreateParent(scenario, Gender.Female);
        var father = hasParents ? CreateParent(scenario, Gender.Male) : null;
        var vat = scenario.Thing(ThingDefOf.GrowthVat).Faction(Faction.OfPlayer).CreateSingle<Building_GrowthVat>();
        var baby = ApplyBirthOutcome(positivityIndex, geneticMother, vat, father);

        Expect.That(baby).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.GaveBirth,
            Description = expectedDescription,
            Concerns = hasParents ? [geneticMother, father] : [geneticMother]
        });
        Expect.That(geneticMother).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.GaveBirth,
            Description = expectedDescription,
            Concerns = hasParents ? [baby, father] : [baby]
        });
    }

    private static Pawn CreateParent(TestScenario scenario, Gender gender)
    {
        return scenario.Pawn()
            .Colonist()
            .SetGender(gender)
            .SetAge(20)
            .CreateSingle();
    }

    private static Pawn ApplyBirthOutcome(int positivityIndex, Pawn geneticMother, Thing birtherThing, Pawn father, float quality = 1f)
    {
        var oldBabiesAreHealthy = Find.Storyteller.difficulty.babiesAreHealthy;

        Find.Storyteller.difficulty.babiesAreHealthy = false;
        try
        {
            var outcome = RitualOutcomeEffectDefOf.ChildBirth.outcomeChances.First(o => o.positivityIndex == positivityIndex);
            var result = PregnancyUtility.ApplyBirthOutcome(
                outcome,
                quality,
                null,
                null,
                geneticMother,
                birtherThing,
                father);

            return result switch
            {
                Pawn pawn => pawn,
                Corpse corpse => corpse.InnerPawn,
                _ => null,
            };
        }
        finally
        {
            Find.Storyteller.difficulty.babiesAreHealthy = oldBabiesAreHealthy;
        }
    }
}
