using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RitualOutcomeComp_GladiatorDuel : RitualOutcomeComp
{
    public override bool Match(BuildInput input) => input.Event.RitualDef == Extra.PreceptDefOf.GladiatorDuel;

    public override IEnumerable<Pawn> GetRecordPawns(BuildInput input)
    {
        yield return GetOrganizer(input);
        yield return GetDuelist1(input);
        yield return GetDuelist2(input);
    }

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder
            .AddRule("Organizer", GetOrganizer(input))
            .AddRule("Duelist1", GetDuelist1(input))
            .AddRule("Duelist2", GetDuelist2(input));
    }

    public override IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        return GetRecordPawns(input);
    }

    private static Pawn GetOrganizer(BuildInput input) => input.Event.AssignedRoles[RitualRoleId.Leader].First();
    private static Pawn GetDuelist1(BuildInput input) => input.Event.AssignedRoles[RitualRoleId.Duelist1].First();
    private static Pawn GetDuelist2(BuildInput input) => input.Event.AssignedRoles[RitualRoleId.Duelist2].First();

    [RequiresIdeology]
    public void Test(TestScenario scenario)
    {
        scenario.SpeedUp();

        var duelIdeo = scenario.Ideo().AddPrecept(Extra.PreceptDefOf.GladiatorDuel).Execute();
        var organizer = scenario.Pawn()
            .Colonist()
            .SetIdeo(duelIdeo, role: PreceptDefOf.IdeoRole_Leader)
            .CreateSingle();
        var escorts = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(duelIdeo)
            .Execute();
        var spectators = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(duelIdeo)
            .Execute();
        var duelists = new List<Pawn>();

        scenario.Map()
            .BuildRoom(11, 11, "arena", floorDef: TerrainDefOf.MetalTile)
            .WithThing(ThingDefOf.RitualSpot, 1, Faction.OfPlayer)
            .Execute();

        scenario.Map()
            .BuildRoom(MapBuilder.Beside("arena", Rot4.East, 6, 6), "prison", floorDef: TerrainDefOf.MetalTile)
            .AsPrison(2, prisoners: duelists)
            .Execute();

        scenario
            .Ritual(organizer)
            .Outcome(Extra.RitualOutcomeEffectDefOf.GladiatorDuel.BestOutcome)
            .GladiatorDuel(duelists[0], duelists[1], escorts[0], escorts[1], spectators)
            .Execute();

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RitualOutcome,
            Description = "[Duelist1] and [Duelist2] fought in an unforgettable [Ritual] organized by [Organizer] in front of 2 others.",
        };

        Expect.That(organizer).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [duelists[0], duelists[1]] }));
        Expect.That(duelists[0]).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [organizer, duelists[1]] }));
        Expect.That(duelists[1]).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [organizer, duelists[0]] }));
        Expect.ThatAll(escorts).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.RitualOutcome);
        Expect.ThatAll(spectators).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.RitualOutcome);
    }
}
