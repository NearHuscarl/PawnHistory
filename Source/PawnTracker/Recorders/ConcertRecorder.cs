using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class ConcertRecorder : RecorderBase<PartyAttendedEvent>
{
    public override void Register()
    {
        if (!ModsConfig.RoyaltyActive)
            return;

        GameEventBus.Subscribe<PartyAttendedEvent>(CreateRecord);
    }

    public override void CreateRecord(PartyAttendedEvent e)
    {
        if (e.Type != PartyType.Concert)
            return;

        var organizer = e.Organizer;

        foreach (var pawn in e.Partygoers)
        {
            if (!ShouldRecord(pawn))
                continue;

            var isOrganizer = pawn == organizer;
            var recordDef = HistoryRecordDefOf.ConcertAttended;
            var desc = recordDef
                .Description(pawn)
                .WithOthers(e.Partygoers)
                .AddRule("Organizer", organizer)
                .AddConstant("isOrganizer", isOrganizer)
                .Resolve();

            AddRecord(recordDef, pawn, desc, [organizer]);
        }
    }

    [RequiresRoyalty]
    public void TestAttended(TestScenario scenario)
    {
        scenario.PartyDuration = PartyRecorder.MinPartyDuration + 30;
        scenario.SpeedUp();
        Expect.Assertions(2);

        var (organizer, lord) = SetupConcert(scenario);

        scenario.WaitUntil(() => lord.CurLordToil is LordToil_End, () =>
        {
            var attendees = lord.ownedPawns.Where(pawn => pawn != organizer).ToList();
            Expect.That(organizer).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.ConcertAttended,
                Description = "[PAWN] held a concert for the colony with [Others].",
            });
            Expect.ThatAny(attendees).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.ConcertAttended,
                Description = "[PAWN] attended [Organizer]'s concert with [Others].",
                Concerns = [organizer],
            });
        });
    }

    private static (Pawn organizer, Lord lord) SetupConcert(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(10, 10)
            .WithThing(Extra.ThingDefOf.Harpsichord, 1, Faction.OfPlayer)
            .Execute();

        scenario.Pawn()
            .Colonist()
            .SetRoyalTitle(Extra.RoyalTitleDefOf.Praetor)
            .CreateSingle();
        scenario.Pawn(3).Colonist().FullHeal().Execute();

        var result = scenario.Incident(Extra.GatheringDefOf.Concert).Execute();
        return (result.Organizers.Single(), result.Lord);
    }
}
