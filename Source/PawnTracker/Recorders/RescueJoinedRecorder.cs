using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class RescueJoinedRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<RescueJoinedEvent>(e =>
        {
            var pawn = e.Pawn;

            if (!ShouldRecord(pawn))
                return;

            HandleRescueJoinedEvent(pawn);
        });
    }

    private void HandleRescueJoinedEvent(Pawn pawn)
    {
        var recordDef = HistoryRecordDefOf.RescueJoined;
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .AddRule("Faction", Faction.OfPlayer)
            .AddConstant("hasFactionName", Faction.OfPlayer.HasName)
            .Resolve();

        AddRecord(recordDef, pawn, desc);
    }

    public override void Test(TestScenario scenario)
    {
        scenario.SpeedUp();
        scenario.Incident(IncidentDefOf.ToxicFallout).Execute(); // pawn will join if it's dangerous outside

        scenario.Map()
            .BuildRoom(7, 7, "Hospital")
            .AsHospital(bedCount: 2)
            .Execute();

        var rescuer = scenario.Pawn().Colonist().CreateSingle();
        var victim = scenario.Pawn()
            .WithFaction(Faction.OfAncients)
            .ThatMatches(ShouldRecord)
            .Do(p => HealthUtility.DamageUntilDowned(p))
            .CreateSingle();

        scenario.Pawn(rescuer)
            .StartJob(JobDefOf.Rescue, victim, RestUtility.FindBedFor(victim, rescuer, checkSocialProperness: false, guestStatus: GuestStatus.Guest))
            .Execute();

        scenario.RunOnceOn<JobEndEvent>(e => e.CurrentJob.def == JobDefOf.Rescue, e =>
        {
            scenario.Pawn(victim).FullHeal().Execute();
            scenario.SlowDown();
        });

        Expect.That(victim).Eventually().ToHaveHistoryRecord("[PAWN] was grateful after being rescued. Instead of leaving, [PAWN_pronoun] decided to stay and joined [F].");
    }
}
