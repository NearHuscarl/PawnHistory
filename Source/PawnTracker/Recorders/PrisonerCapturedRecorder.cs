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

public class PrisonerCapturedRecorder : RecorderBase<PrisonerCapturedRecorder.Input>
{
    public record Input(Pawn Prisoner, Pawn Captor, Quest Quest, string Quality);

    public override void Register()
    {
        GameEventBus.Subscribe<PrisonerCapturedEvent>(e =>
        {
            if (e.Prisoner.GetRoom() is not { } room)
                return;
            var impressiveScore = room.GetStat(RoomStatDefOf.Impressiveness);
            var quality = RoomStatDefOf.Impressiveness.GetScoreStage(impressiveScore).label;

            CreateRecord(new Input(e.Prisoner, e.Captor, e.Quest, quality));
        });
    }

    public override void CreateRecord(Input input)
    {
        var (prisoner, captor, quest, quality) = input;
        var recordDef = HistoryRecordDefOf.PrisonerCaptured;
        var desc = GetDescription(captor, prisoner, quality);

        AddRecord(recordDef, captor, desc, [prisoner], quest: quest);
        AddRecord(recordDef, prisoner, desc, [captor], quest: quest);
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
                prisoner.guest.CapturedBy(captor.Faction, captor);
                
                var expected = new ExpectedHistoryRecord
                {
                    Def = HistoryRecordDefOf.PrisonerCaptured,
                    Description = "[Prisoner], a member of [HostileFaction], was put into [RoomQuality_indefinite] prison.",
                    Quest = quest,
                };
                Expect.That(prisoner).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [captor] }));
                Expect.That(captor).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [prisoner] }));
                scenario.SlowDown();
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
