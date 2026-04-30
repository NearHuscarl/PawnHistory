using PawnHistory.Source.DebugTools;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PrisonerCapturedRecorder : RecorderBase<PrisonerCapturedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<PrisonerCapturedEvent>(CreateRecord);
    }

    public override void CreateRecord(PrisonerCapturedEvent e)
    {
        var (prisoner, faction,  captor, quest) = e;
        var recordDef = HistoryRecordDefOf.PrisonerCaptured;
        
        var desc = GetDescription(captor, prisoner, faction);

        if (ShouldRecord(captor))
            AddRecord(recordDef, captor, desc, [prisoner], quest: quest);
        AddRecord(recordDef, prisoner, desc, [captor], quest: quest);
    }

    private string GetDescription(Pawn captor, Pawn prisoner, Faction faction, bool testPermutation = false)
    {
        var recordDef = HistoryRecordDefOf.PrisonerCaptured;
        // Auto captured by moving a non-prisoner to a caravan of different faction.
        var isAutoCapturedByCaravan = captor == null;
        string quality = null;

        if (prisoner.GetRoom() is { } room)
        {
            var impressiveScore = room.GetStat(RoomStatDefOf.Impressiveness);
            quality = RoomStatDefOf.Impressiveness.GetScoreStage(impressiveScore).label.ToLower();
        }

        return recordDef.Description(prisoner, "Prisoner")
            .AddRule("Captor", captor, addSubsymbols: true)
            .AddRule("HostileFaction", prisoner.Faction)
            .AddRule("CaptureFaction", faction)
            .AddRule("RoomQuality", quality, addSubsymbols: true)
            .AddConstant("autoCaptured", isAutoCapturedByCaravan)
            .AddConstant("testPermutation", testPermutation)
            .Resolve();
    }

    public void Test(TestScenario scenario)
    {
        scenario.SpeedUp();

        scenario.Map()
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

        scenario.RunOnceOn<PrisonerCapturedEvent>(_ =>
        {
            var expected = new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.PrisonerCaptured,
                Description = "[Prisoner], a member of [HostileFaction], was put into [RoomQuality_indefinite] prison."
            };
            Expect.That(prisoner).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [captor] }));
            Expect.That(captor).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [prisoner] }));
            scenario.SlowDown();
        });
    }

    public void TestQuest(TestScenario scenario)
    {
        Rand.PushState(12345); // site occasionally doesn't spawn prison due to small map size
        var quest = scenario.Quest(Extra.QuestScriptDefOf.OpportunitySite_PrisonerWillingToJoin).Execute();
        
        var site = QuestHelper.GetWorldObject<Site>(quest);
        var captor = scenario.Pawn().Colonist().CreateSingle();
        var prisoner = QuestHelper.GetPawnReward(quest);

        Rand.PopState();
        scenario.Caravan([captor]).VisitSite(site)
            .OnMapGenerated(e =>
            {
                scenario.Map(e.Map).ClaimAllBuildings().Execute();
                e.Map.mapPawns.FreeHumanlikesSpawnedOfFaction(e.MapParent.Faction).ForEach(p => p.Kill(null));
                prisoner.guest.CapturedBy(captor.Faction, captor);
                
                var expected = new ExpectedHistoryRecord
                {
                    Def = HistoryRecordDefOf.PrisonerCaptured,
                    Description = "[Prisoner], a member of [HostileFaction], was put into [RoomQuality_indefinite] prison.",
                    Quest = quest,
                };
                Expect.That(prisoner).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [captor] }));
                Expect.That(captor).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [prisoner] }));
            })
            .Execute();
    }

    public void TestAutoCapture(TestScenario scenario)
    {
        Rand.PushState(12345); // site occasionally doesn't spawn prison due to small map size
        var quest = scenario.Quest(Extra.QuestScriptDefOf.OpportunitySite_PrisonerWillingToJoin).Execute();
        
        var site = QuestHelper.GetWorldObject<Site>(quest);
        var captor = scenario.Pawn().Colonist().CreateSingle();
        var prisoner = QuestHelper.GetPawnReward(quest);

        Rand.PopState();
        scenario.Caravan([captor]).VisitSite(site)
            .OnMapGenerated(e =>
            {
                scenario.Map(e.Map).ClaimAllBuildings().Execute();
                e.Map.mapPawns.FreeHumanlikesSpawnedOfFaction(e.MapParent.Faction).ForEach(p => p.Kill(null));
                HealthUtility.DamageUntilDowned(prisoner);

                scenario.Caravan([captor, prisoner]).Execute();
                Expect.That(prisoner).ToHaveHistoryRecord(new ExpectedHistoryRecord
                {
                    Def = HistoryRecordDefOf.PrisonerCaptured,
                    Description = "[Prisoner] was captured by [CaptureFaction]'s caravan.",
                    Quest = quest,
                });
            })
            .Execute();
    }

    [SkipTest]
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

        for (var i = 0; i < n; i++)
        {
            var desc = GetDescription(captor, prisoner,  Faction.OfPlayer, testPermutation: true);
            descriptions.Add(desc);
        }

        Log.Message("=== ALL DESCRIPTIONS ===");
        for (var i = 0; i < descriptions.Count; i++)
        {
            Log.Message($"[{i}] {descriptions[i]}");
        }

        var overlaps = new List<(int i, int j, float score)>();
        for (var i = 0; i < descriptions.Count; i++)
        {
            for (var j = i + 1; j < descriptions.Count; j++)
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
