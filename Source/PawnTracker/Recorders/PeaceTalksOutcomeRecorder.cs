using HarmonyLib;
using System.Linq;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PeaceTalksOutcomeRecorder : RecorderBase<PeaceTalksOutcomeEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<PeaceTalksOutcomeEvent>(CreateRecord);
    }

    public override void CreateRecord(PeaceTalksOutcomeEvent input)
    {
        if (!ShouldRecord(input.Negotiator))
            return;

        RecordNegotiator(input);

        if (input.Outcome == PeaceTalksOutcome.Disaster)
            RecordEnemyRaid(input);
    }

    private void RecordNegotiator(PeaceTalksOutcomeEvent input)
    {
        var (negotiator, faction, outcome, _) = input;
        var recordDef = HistoryRecordDefOf.PeaceTalksOutcome;
        var desc = recordDef.Description(negotiator)
            .AddRule("Faction", faction)
            .AddConstant("outcome", outcome)
            .Resolve();

        AddRecord(recordDef, negotiator, desc);
    }

    private void RecordEnemyRaid(PeaceTalksOutcomeEvent input)
    {
        var enemies = input.Enemies?.Where(ShouldRecord).ToList() ?? [];
        if (enemies.Count == 0)
            return;

        var recordDef = HistoryRecordDefOf.PeaceTalksRaid;

        foreach (var pawn in enemies)
        {
            var desc = recordDef.Description(pawn)
                .WithOthers(enemies)
                .AddRule("Faction", input.Faction, addSubsymbols: true)
                .AddConstant("outcome", input.Outcome)
                .Resolve();

            AddRecord(recordDef, pawn, desc, [input.Negotiator]);
        }
    }

    private static Pawn SetupPeaceTalkOutcome(TestScenario scenario, PeaceTalksOutcome outcome)
    {
        scenario.Quest(DefLookup.QuestScript.OpportunitySite_PeaceTalks).Execute();
        var peaceTalks = Find.WorldObjects.AllWorldObjects.OfType<PeaceTalks>().First();
        var pawn = scenario.Pawn().Colonist().Execute().First();
        var caravan = scenario.Caravan([pawn]).Position(peaceTalks.Tile).Execute();

        AccessTools.Method(typeof(PeaceTalks), $"Outcome_{outcome}").Invoke(peaceTalks, [caravan]);
        var negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);

        return negotiator;
    }

    public void TestBackfire(TestScenario scenario)
    {
        var negotiator = SetupPeaceTalkOutcome(scenario, PeaceTalksOutcome.Backfire);

        Expect.That(negotiator).ToHaveHistoryRecord("[PAWN] negotiated peace talks with [Faction], but they backfired.", HistoryRecordDefOf.PeaceTalksOutcome);
    }

    public void TestTalksFlounder(TestScenario scenario)
    {
        var negotiator = SetupPeaceTalkOutcome(scenario, PeaceTalksOutcome.TalksFlounder);

        Expect.That(negotiator).ToHaveHistoryRecord("[PAWN] negotiated peace talks with [Faction], but they floundered.", HistoryRecordDefOf.PeaceTalksOutcome);
    }

    public void TestSuccess(TestScenario scenario)
    {
        var negotiator = SetupPeaceTalkOutcome(scenario, PeaceTalksOutcome.Success);

        Expect.That(negotiator).ToHaveHistoryRecord("[PAWN] successfully negotiated peace talks with [Faction].", HistoryRecordDefOf.PeaceTalksOutcome);
    }

    public void TestTriumph(TestScenario scenario)
    {
        var negotiator = SetupPeaceTalkOutcome(scenario, PeaceTalksOutcome.Triumph);

        Expect.That(negotiator).ToHaveHistoryRecord("[PAWN] negotiated peace talks with [Faction] to a great triumph.", HistoryRecordDefOf.PeaceTalksOutcome);
    }

    public void TestDisaster(TestScenario scenario)
    {
        Expect.Assertions(2);

        var negotiator = SetupPeaceTalkOutcome(scenario, PeaceTalksOutcome.Disaster);
        
        GameEventBus.SubscribeOnce<LordToilChangeEvent>(e =>
        {
            if (e.Lord.LordJob is not LordJob_AssaultColony)
                return;

            var enemies = e.Lord.ownedPawns;
            Expect.ThatAll(enemies).ToHaveHistoryRecord("[PAWN] from [Faction] attacked the peace talks delegation.", HistoryRecordDefOf.PeaceTalksRaid);
        });

        Expect.That(negotiator).ToHaveHistoryRecord("[PAWN] negotiated peace talks with [Faction], but they ended in disaster.", HistoryRecordDefOf.PeaceTalksOutcome);
    }
}
