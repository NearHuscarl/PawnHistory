using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RitualOutcomeComp_Festival : RitualOutcomeComp
{
    public override bool Match(BuildInput input) => input.Event.RitualDef == Extra.PreceptDefOf.Festival;

    public override bool RecordParticipants { get; protected set; } = true;

    [RequiresIdeology]
    public void Test(TestScenario scenario)
    {
        scenario.SpeedUp();
        scenario.ForwardDays(60); // forward a year as this is a DateRitual 

        var festivalIdeo = scenario.Ideo().AddPrecept(Extra.PreceptDefOf.Festival).Execute();
        var organizer = scenario.Pawn()
            .Colonist()
            .SetIdeo(festivalIdeo, role: PreceptDefOf.IdeoRole_Leader)
            .CreateSingle();
        var joiners = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(festivalIdeo)
            .Execute();
        var participants = joiners.Concat(organizer);

        scenario.Map()
            .BuildRoom(8, 8)
            .WithThing(ThingDefOf.PartySpot, 1, Faction.OfPlayer)
            .Execute();

        scenario
            .Ritual(organizer)
            .Outcome(Extra.RitualOutcomeEffectDefOf.CelebratedDate.BestOutcome)
            .Festival(joiners)
            .Execute();

        Expect.ThatAll(participants).ToHaveHistoryRecord(HistoryRecordDefOf.RitualOutcome, "[PAWN] attended an unforgettable [Ritual] with 2 others.");
    }
}
