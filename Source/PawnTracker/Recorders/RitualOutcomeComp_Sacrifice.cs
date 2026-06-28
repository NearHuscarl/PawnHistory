using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RitualOutcomeComp_Sacrifice : RitualOutcomeComp
{
    public override bool Match(BuildInput input) => ModsConfig.IdeologyActive && input.Event.OutcomeEffectDef == Extra.RitualOutcomeEffectDefOf.Sacrifice;

    public override IEnumerable<Pawn> GetRecordPawns(BuildInput input)
    {
        yield return GetExecutioner(input);
        yield return GetVictim(input);
    }

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder
            .AddConstant("ritual", input.Event.OutcomeEffectDef.defName)
            .AddRule("Executioner", GetExecutioner(input))
            .AddRule("Victim", GetVictim(input));
    }

    public override IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        yield return GetExecutioner(input);
        yield return GetVictim(input);
    }

    private static Pawn GetExecutioner(BuildInput input) => input.Event.AssignedRoles[RitualRoleId.Moralist].First();

    private static Pawn GetVictim(BuildInput input)
    {
        // handles both SacrificePrisoner and SacrificeAnimal with different role ids.
        if (input.Event.AssignedRoles.TryGetValue(RitualRoleId.Prisoner, out var prisoners))
            return prisoners.First();

        return input.Event.AssignedRoles[RitualRoleId.Animal].First();
    }

    [RequiresIdeology]
    public void TestPrisoner(TestScenario scenario)
    {
        scenario.SpeedUp();
        scenario.ForwardDays(60);

        var sacrificeIdeo = scenario.Ideo()
            .AddPrecept(Extra.PreceptDefOf.Festival, Extra.RitualPatternDefOf.SacrificePrisoner)
            .Execute();
        var executioner = scenario.Pawn()
            .Colonist()
            .SetIdeo(sacrificeIdeo, role: PreceptDefOf.IdeoRole_Moralist)
            .CreateSingle();
        var spectators = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(sacrificeIdeo)
            .Execute();
        var prisoners = new List<Pawn>();

        scenario.Map()
            .BuildRoom(8, 8, "prison")
            .AsPrison(1, prisoners: prisoners)
            .Execute();

        scenario.Map()
            .BuildRoom(MapBuilder.Beside("prison", Rot4.East, 8, 8), "ritual")
            .WithThing(ThingDefOf.RitualSpot, 1, Faction.OfPlayer)
            .Execute();

        var prisoner = prisoners[0];

        scenario
            .Ritual(executioner)
            .Outcome(Extra.RitualOutcomeEffectDefOf.Sacrifice.WorstOutcome)
            .SacrificePrisoner(prisoner, spectators)
            .Execute();

        var expected = new ExpectedHistoryRecord()
        {
            Def = HistoryRecordDefOf.RitualOutcome,
            Description = "[Victim] was sacrificed by [Executioner] during a terrible [Ritual] in front of 2 others.",
        };
        Expect.That(executioner).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [prisoner] }));
        Expect.That(prisoner).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [executioner] }));
        Expect.ThatAll(spectators).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.RitualOutcome);
    }

    [RequiresIdeology]
    public void TestAnimal(TestScenario scenario)
    {
        scenario.SpeedUp();
        scenario.ForwardDays(60);

        var sacrificeIdeo = scenario.Ideo()
            .AddPrecept(Extra.PreceptDefOf.Festival, Extra.RitualPatternDefOf.SacrificeAnimal)
            .Execute();
        var animal = scenario.Pawn()
            .Animal(PawnKindDefOf.Muffalo)
            .WithFaction(Faction.OfPlayer)
            .CreateSingle();
        var executioner = scenario.Pawn()
            .Colonist()
            .SetIdeo(sacrificeIdeo, role: PreceptDefOf.IdeoRole_Moralist)
            .SetRelation(animal, PawnRelationDefOf.Bond)
            .CreateSingle();
        var spectators = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(sacrificeIdeo)
            .Execute();

        scenario.Map()
            .BuildRoom(8, 8)
            .WithThing(ThingDefOf.RitualSpot, 1, Faction.OfPlayer)
            .Execute();

        scenario
            .Ritual(executioner)
            .Outcome(Extra.RitualOutcomeEffectDefOf.Sacrifice.WorstOutcome)
            .SacrificeAnimal(animal, spectators)
            .Execute();

        var expected = new ExpectedHistoryRecord()
        {
            Def = HistoryRecordDefOf.RitualOutcome,
            Description = "[Victim] was sacrificed by [Executioner] during a terrible [Ritual] in front of 2 others.",
        };
        Expect.That(executioner).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [animal] }));
        Expect.That(animal).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [executioner] }));
        Expect.ThatAll(spectators).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.RitualOutcome);
    }
}
