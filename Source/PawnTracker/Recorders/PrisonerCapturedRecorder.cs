using PawnHistory.Source.DebugTools;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class PrisonerCapturedRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<PrisonerCapturedEvent>(e =>
        {
            HandleCapturedEvent(e);
        });
    }

    private void HandleCapturedEvent(PrisonerCapturedEvent e)
    {
        var recordDef = HistoryRecordDefOf.PrisonerCaptured;
        var impressiveScore = e.Room.GetStat(RoomStatDefOf.Impressiveness);
        var quality = RoomStatDefOf.Impressiveness.GetScoreStage(impressiveScore).label;
        var desc = recordDef.Description(e.Captor, "Captor")
            .AddRule("Prisoner", e.Prisoner, addSubsymbols: true)
            .AddRule("HostileFaction", e.Prisoner.Faction)
            .AddRule("RoomQuality", quality.ToLower(), addSubsymbols: true)
            .Resolve();

        AddRecord(recordDef, e.Captor, desc, [e.Prisoner]);
        AddRecord(recordDef, e.Prisoner, desc, [e.Captor]);
    }

    public override void Test(TestScenario scenario)
    {
        NearDebugSettings.NoDisabledWorkTypes = true;
        Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;

        scenario.Thing()
            .BuildRoom(8, 8, tag: "Prison")
            .AsPrison(prisonerCount: 0, bedCount: 2)
            .Execute();
        var prisoner = scenario.Pawn()
            .WithFaction(Faction.OfPirates)
            .Do(p => HealthUtility.DamageUntilDowned(p))
            .CreateSingle();
        var captor = scenario.Pawn()
            .Colonist()
            .Capture(prisoner)
            .CreateSingle();

        GameEventBus.RunOnceWhen<PrisonerCapturedEvent>((e) => true, e =>
        {
            NearDebugSettings.NoDisabledWorkTypes = false;
            Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
            scenario.OpenHistoryRecordTab(prisoner);
        });
    }
}
