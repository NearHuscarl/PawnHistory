using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RitualOutcomeComp_Festival : RitualOutcomeComp
{
    public override bool Match(BuildInput input)
    {
        return input.Event.RitualDef == Extra.PreceptDefOf.Festival
            || input.Event.RitualDef == Extra.PreceptDefOf.Classic_DrumParty
            || input.Event.RitualDef == Extra.PreceptDefOf.Classic_DanceParty;
    }

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
            .Outcome(BestOutcomeFor(festivalIdeo, Extra.PreceptDefOf.Festival))
            .Festival(joiners)
            .Execute();

        Expect.ThatAll(participants).ToHaveHistoryRecord(HistoryRecordDefOf.RitualOutcome, "[PAWN] attended an unforgettable [Ritual] with 2 others.");
    }

    [RequiresIdeology]
    public void TestClassicDrumParty(TestScenario scenario)
    {
        scenario.SpeedUp();
        scenario.ForwardDays(60);

        var drumPartyIdeo = scenario.Ideo().AddPrecept(Extra.PreceptDefOf.Classic_DrumParty).Execute();
        var organizer = scenario.Pawn()
            .Colonist()
            .SetIdeo(drumPartyIdeo, role: PreceptDefOf.IdeoRole_Leader)
            .CreateSingle();
        var joiners = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(drumPartyIdeo)
            .Execute();
        var participants = joiners.Concat(organizer);

        scenario.Map()
            .BuildRoom(8, 8)
            .WithThing(ThingDefOf.RitualSpot, 1, Faction.OfPlayer)
            .WithThing(ThingDefOf.Campfire, 1, Faction.OfPlayer)
            .WithThing(ThingDefOf.Drum, 1, Faction.OfPlayer, ThingDefOf.Cloth)
            .Execute();

        scenario
            .Ritual(organizer)
            .Outcome(BestOutcomeFor(drumPartyIdeo, Extra.PreceptDefOf.Classic_DrumParty))
            .DrumParty(joiners)
            .Execute();

        Expect.ThatAll(participants).ToHaveHistoryRecord(HistoryRecordDefOf.RitualOutcome, "[PAWN] attended an unforgettable [Ritual] with 2 others.");
    }

    [RequiresIdeology]
    public void TestClassicDanceParty(TestScenario scenario)
    {
        scenario.SpeedUp();
        scenario.ForwardDays(60);

        var dancePartyIdeo = scenario.Ideo().AddPrecept(Extra.PreceptDefOf.Classic_DanceParty).Execute();
        var organizer = scenario.Pawn()
            .Colonist()
            .SetIdeo(dancePartyIdeo, role: PreceptDefOf.IdeoRole_Leader)
            .CreateSingle();
        var joiners = scenario.Pawn(2)
            .Colonist()
            .SetIdeo(dancePartyIdeo)
            .Execute();
        var participants = joiners.Concat(organizer);

        scenario.Map()
            .BuildRoom(8, 8)
            .WithThing(ThingDefOf.RitualSpot, 1, Faction.OfPlayer)
            .WithThing(Extra.ThingDefOf.VanometricPowerCell, 1, Faction.OfPlayer)
            .WithThing(ThingDefOf.LightBall, 1, Faction.OfPlayer)
            .WithThing(ThingDefOf.Loudspeaker, 1, Faction.OfPlayer)
            .Execute();

        scenario
            .Ritual(organizer)
            .Outcome(BestOutcomeFor(dancePartyIdeo, Extra.PreceptDefOf.Classic_DanceParty))
            .DanceParty(joiners)
            .Execute();

        Expect.ThatAll(participants).ToHaveHistoryRecord(HistoryRecordDefOf.RitualOutcome, "[PAWN] attended an unforgettable [Ritual] with 2 others.");
    }

    private static RitualOutcomePossibility BestOutcomeFor(Ideo ideo, PreceptDef ritualDef)
    {
        var ritual = (Precept_Ritual)ideo.GetPrecept(ritualDef);
        return ritual.outcomeEffect.def.BestOutcome;
    }
}
