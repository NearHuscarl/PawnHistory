using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class ConcertRecorder : HistoryTaleRecorder<ConcertAttendedEvent>
{
    public override void Register()
    {
        if (!ModsConfig.RoyaltyActive)
            return;

        GameEventBus.Subscribe<ConcertAttendedEvent>(CreateRecord);
    }

    public override void CreateRecord(ConcertAttendedEvent e)
    {
        var (attender, organizer) = e;
        if (!ShouldRecord(attender))
            return;

        var recordDef = HistoryRecordDefOf.ConcertAttended;
        var desc = recordDef
            .Description(attender, "Attender")
            .IncludePawnGrammar()
            .AddRule("Organizer", organizer)
            .Resolve();

        if (!ShouldRecordTale(attender, recordDef, desc))
            return;

        AddRecord(recordDef, attender, desc, [organizer]);
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
            Expect.That(organizer).ToHaveHistoryRecordOf(HistoryRecordDefOf.ConcertHeld);
            Expect.ThatAny(attendees).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.ConcertAttended,
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
        scenario.Pawn(10).Colonist().FullHeal().Execute();

        var result = scenario.Incident(Extra.GatheringDefOf.Concert).Execute();
        return (result.Organizers.Single(), result.Lord);
    }
}
