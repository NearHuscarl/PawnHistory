using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.HistoryBackfill;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PsylinkLevelGainedRecorder : RecorderBase<PsylinkLevelGainedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<PsylinkLevelGainedEvent>(CreateRecord);
    }

    internal override IEnumerable<HistoryBackfillDefinition> GetBackfillDefinitions()
    {
        if (!ModsConfig.RoyaltyActive)
            yield break;

        yield return new HistoryBackfillDefinition(HistoryRecordDefOf.PsylinkLevelGained)
            .AddHard(
                new MinimumAgeRule(13f),
                new MaximumCountRule(1),
                new LogicalGateRule((_, _) => ModsConfig.RoyaltyActive))
            .AddSoft(new ShiftedAgeCurveSoftRule([
                new CurvePoint(13f, 0.02f),
                new CurvePoint(18f, 0.1f),
                new CurvePoint(25f, 0.4f),
                new CurvePoint(35f, 1f),
                new CurvePoint(50f, 1.1f),
                new CurvePoint(80f, 0.5f)
            ], 6f));
    }

    public override void CreateRecord(PsylinkLevelGainedEvent e)
    {
        if (e?.Pawn == null)
            return;

        if (!ShouldRecord(e.Pawn))
            return;

        // TODO: handle multiple new abilities in RitualOutcomeEffectWorker_Bestowing
        var recordDef = HistoryRecordDefOf.PsylinkLevelGained;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddRule("NewLevel", e.NewLevel)
            .AddRule("NewAbility", e.NewAbility?.label.Colorize(ColoredText.TipSectionTitleColor))
            .AddConstant("hasNewAbility", e.NewAbility != null)
            .AddConstant("isFirstLvl", e.NewLevel == 1)
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc);
    }

    [RequiresRoyalty]
    public void Test(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().Do(p => p.ChangePsylinkLevel(1)).CreateSingle();
        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.PsylinkLevelGained, "[PAWN] gained [His] first psylink level and became a psycaster. [He] learned a new psychic power: [NewAbility].");
    }

    [RequiresRoyalty]
    public void TestNextLevel(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().Do(p => p.ChangePsylinkLevel(1)).CreateSingle();

        scenario.Pawn(pawn).Do(p => p.ChangePsylinkLevel(1)).Execute();

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.PsylinkLevelGained, "[PAWN] advanced to psylink level 2. [He] learned a new psychic power: [NewAbility].");
    }
}
