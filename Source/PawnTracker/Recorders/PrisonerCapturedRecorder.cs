using PawnHistory.Source.DebugTools;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
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

    private string GetDescription(Pawn captor, Pawn prisoner, string quality, bool testPermutation = false)
    {
        var recordDef = HistoryRecordDefOf.PrisonerCaptured;

        return recordDef.Description(captor, "Captor")
            .AddRule("Prisoner", prisoner, addSubsymbols: true)
            .AddRule("HostileFaction", prisoner.Faction)
            .AddRule("RoomQuality", quality.ToLower(), addSubsymbols: true)
            .AddConstant("testPermutation", testPermutation)
            .Resolve();
    }

    private void HandleCapturedEvent(PrisonerCapturedEvent e)
    {
        var recordDef = HistoryRecordDefOf.PrisonerCaptured;
        var impressiveScore = e.Room.GetStat(RoomStatDefOf.Impressiveness);
        var quality = RoomStatDefOf.Impressiveness.GetScoreStage(impressiveScore).label;
        var desc = GetDescription(e.Captor, e.Prisoner, quality);

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

    public void TestArrest(TestScenario scenario)
    {
        NearDebugSettings.NoDisabledWorkTypes = true;
        Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;

        scenario.Thing()
            .BuildRoom(8, 8, tag: "Prison")
            .AsPrison(prisonerCount: 0, bedCount: 2)
            .Execute();
        var friend = scenario.Pawn()
            .Colonist()
            .Do(p => HealthUtility.DamageUntilDowned(p))
            .CreateSingle();
        var captor = scenario.Pawn()
            .Colonist()
            .Do(p => CaptureUtility.OrderArrest(p, friend))
            .CreateSingle();

        GameEventBus.RunOnceWhen<PrisonerCapturedEvent>((e) => true, e =>
        {
            NearDebugSettings.NoDisabledWorkTypes = false;
            Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
            scenario.OpenHistoryRecordTab(friend);
        });
    }

    public void TestPermutation(TestScenario scenario)
    {
        const int n = 20;

        var prisoner = scenario.Pawn()
            .WithFaction(Faction.OfPirates)
            .CreateSingle();

        var captor = scenario.Pawn()
            .Colonist()
            .CreateSingle();

        var descriptions = new List<string>();

        for (int i = 0; i < n; i++)
        {
            var desc = GetDescription(captor, prisoner, "decent", testPermutation: true);
            descriptions.Add(desc);
        }

        Log.Message("=== ALL DESCRIPTIONS ===");
        for (int i = 0; i < descriptions.Count; i++)
        {
            Log.Message($"[{i}] {descriptions[i]}");
        }

        var overlaps = new List<(int i, int j, float score)>();
        for (int i = 0; i < descriptions.Count; i++)
        {
            for (int j = i + 1; j < descriptions.Count; j++)
            {
                var score = LangUtility.GetOverlapScore(descriptions[i], descriptions[j]);
                overlaps.Add((i, j, score));
            }
        }

        var topOverlaps = overlaps
            .OrderByDescending(x => x.score)
            .Take(20)
            .ToList();

        Log.Message("=== TOP 20 OVERLAPS ===");
        foreach (var (i, j, score) in topOverlaps)
        {
            Log.Message($"[{i}] vs [{j}] => Score: {score}");
        }

        var mean = overlaps.Average(x => x.score);
        var median = overlaps.Median(x => x.score);

        Log.Message("=== OVERLAP STATS ===");
        Log.Message($"Pairs: {overlaps.Count}");
        Log.Message($"Mean overlap: {mean}");
        Log.Message($"Median overlap: {median}");
    }
}
