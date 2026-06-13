using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PartyRecorder : HistoryTaleRecorder<AttendedPartyEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<AttendedPartyEvent>(CreateRecord);
    }

    public override void CreateRecord(AttendedPartyEvent e)
    {
        var (attender, organizer) = e;
        if (!ShouldRecord(attender))
            return;

        var isOrganizer = attender == organizer;
        var recordDef = HistoryRecordDefOf.PartyAttended;
        var desc = recordDef
            .Description(attender, "Attender")
            .IncludePawnGrammar()
            .AddRule("Organizer", organizer)
            .AddConstant("isOrganizer", isOrganizer)
            .Resolve();

        if (!ShouldRecordTale(attender, recordDef, desc))
            return;

        AddRecord(recordDef, attender, desc, [organizer]);
    }

    public static readonly int MinPartyDuration = 1200; // must larger than this value in IsGatheringAboutToEnd()

    public void TestAttended(TestScenario scenario)
    {
        scenario.PartyDuration = MinPartyDuration + 30;
        scenario.SpeedUp();

        Expect.Assertions(2);
        var (organizer, lord) = SetupParty(scenario);

        scenario.WaitUntil(() => lord.CurLordToil is LordToil_End, () =>
        {
            var attendees = lord.ownedPawns.Where(p => p != organizer).ToList();

            Expect.That(organizer).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.PartyAttended,
                Description = "[Attender] threw a party for the colony.",
            });
            Expect.ThatAny(attendees).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.PartyAttended,
                Concerns = [organizer],
            });
        });
    }

    public Action TestCancelled(TestScenario scenario)
    {
        scenario.SpeedUp();
        Expect.Assertions(1);
        var (_, lord) = SetupParty(scenario);

        scenario.WaitUntil(() => lord.CurLordToil is LordToil_End, () =>
        {
            var partyGoers = lord.ownedPawns.ToList();
            Expect.ThatAll(partyGoers).Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.PartyAttended);
        });

        TickDelayManager.Delay(200, () =>
        {
            scenario.Incident(IncidentDefOf.RaidEnemy).Point(500).Execute();
            scenario.RaidFriendly().Point(500).Execute();
        });

        return () => scenario.SlowDown();
    }

    private static (Pawn organizer, Lord lord) SetupParty(TestScenario scenario)
    {
        scenario.Map().BuildRoom(8, 8).WithThing(ThingDefOf.PartySpot, 1, Faction.OfPlayer).Execute();
        scenario.Pawn(8).Colonist().Execute();
        var result = scenario.Incident(GatheringDefOf.Party).Execute();

        // mocked party ends almost immediately. Force joining party to be registered.
        foreach (var pawn in Find.CurrentMap.mapPawns.FreeColonistsSpawned.ToList())
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, startNewJob: true);

        return (result.Organizers.Single(), result.Lord);
    }
}
