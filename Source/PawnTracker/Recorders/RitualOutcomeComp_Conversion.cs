using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RitualOutcomeComp_Conversion : RitualOutcomeComp
{
    public override bool Match(BuildInput input) => input.Event.RitualDef == Extra.PreceptDefOf.Conversion;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var convertee = input.Event.AssignedRoles[RitualRoleId.Convertee].First();

        return builder.AddRule("Convertee", convertee, addSubsymbols: true);
    }

    public override IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        yield return input.Event.AssignedRoles[RitualRoleId.Convertee].First();
    }

    [RequiresIdeology]
    public void Test(TestScenario scenario)
    {
        scenario.SpeedUp();

        var organizer = scenario.Pawn()
            .Colonist()
            .SetIdeo(role: PreceptDefOf.IdeoRole_Moralist)
            .CreateSingle();
        var converted = scenario.Pawn()
            .Colonist()
            .SetIdeo(Faction.OfHostile.ideos.PrimaryIdeo, certainty: 0.1f)
            .CreateSingle();
        var spectators = scenario.Pawn(2).Colonist().Execute();

        scenario.Map()
            .BuildRoom(8, 8)
            .AsShrine(organizer.Ideo)
            .Execute();

        scenario
            .Ritual(organizer)
            .Outcome(Extra.RitualOutcomeEffectDefOf.Conversion.BestOutcome)
            .ConversionRitual(converted, spectators)
            .Execute();

        Expect.That(organizer).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RitualOutcome,
            Description = "[PAWN] delivered a masterful conversion ritual to bring [Convertee] into [His] ideoligion before 2 others.",
            Concerns = [converted],
        });
    }
}
