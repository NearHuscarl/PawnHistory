using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker;

internal static class HistoryBackfillRegistry
{
    private static readonly DensityGlobalRule HealthDensityRule = new("HealthPrehistory");
    private static readonly Dictionary<HistoryRecordDef, HistoryBackfillDefinition> Definitions = BuildDefinitions();

    public static IReadOnlyCollection<HistoryRecordDef> ManagedDefs => Definitions.Keys.ToList();

    public static bool TryGetDefinition(HistoryRecordDef def, out HistoryBackfillDefinition definition) => Definitions.TryGetValue(def, out definition);

    private static Dictionary<HistoryRecordDef, HistoryBackfillDefinition> BuildDefinitions()
    {
        return new()
        {
            {
                HistoryRecordDefOf.TitleGained,
                new HistoryBackfillDefinition(HistoryRecordDefOf.TitleGained)
                    .AddHard(
                        new MinimumAgeRule(13f),
                        new MaximumCountRule(1),
                        new LogicalGateRule((_, _) => ModsConfig.RoyaltyActive),
                        new OrderBeforeRule(GenDate.TicksPerDay, HistoryRecordDefOf.PsylinkLevelGained))
                    .AddSoft(new AgeCurveSoftRule([
                        new CurvePoint(13f, 0.02f),
                        new CurvePoint(18f, 0.25f),
                        new CurvePoint(25f, 1f),
                        new CurvePoint(35f, 1.2f),
                        new CurvePoint(55f, 0.75f),
                        new CurvePoint(90f, 0.2f)
                    ]))
            },
            {
                HistoryRecordDefOf.PsylinkLevelGained,
                new HistoryBackfillDefinition(HistoryRecordDefOf.PsylinkLevelGained)
                    .AddHard(
                        new MinimumAgeRule(13f),
                        new LogicalGateRule((_, _) => ModsConfig.RoyaltyActive),
                        new SiblingSequenceRule(GenDate.TicksPerDay))
                    .AddSoft(new ShiftedAgeCurveSoftRule([
                        new CurvePoint(13f, 0.02f),
                        new CurvePoint(18f, 0.1f),
                        new CurvePoint(25f, 0.4f),
                        new CurvePoint(35f, 1f),
                        new CurvePoint(50f, 1.1f),
                        new CurvePoint(80f, 0.5f)
                    ], 6f))
            },
            {
                HistoryRecordDefOf.BodyPartScarred,
                new HistoryBackfillDefinition(HistoryRecordDefOf.BodyPartScarred, "HealthPrehistory")
                    .AddHard(
                        new MinimumAgeRule(7f),
                        new SiblingSequenceRule(GenDate.DaysToTicks(45f)))
                    .AddSoft(new AgeCurveSoftRule([
                        new CurvePoint(7f, 0.02f),
                        new CurvePoint(13f, 0.12f),
                        new CurvePoint(18f, 0.4f),
                        new CurvePoint(30f, 1f),
                        new CurvePoint(55f, 1.15f),
                        new CurvePoint(90f, 0.6f)
                    ]))
                    .AddGlobal(HealthDensityRule)
            },
            {
                HistoryRecordDefOf.BodyPartDestroyed,
                new HistoryBackfillDefinition(HistoryRecordDefOf.BodyPartDestroyed, "HealthPrehistory")
                    .AddHard(
                        new MinimumAgeRule(7f),
                        new SiblingSequenceRule(GenDate.DaysToTicks(90f)))
                    .AddSoft(new AgeCurveSoftRule([
                        new CurvePoint(7f, 0.01f),
                        new CurvePoint(16f, 0.08f),
                        new CurvePoint(22f, 0.25f),
                        new CurvePoint(35f, 0.9f),
                        new CurvePoint(55f, 1.2f),
                        new CurvePoint(90f, 0.65f)
                    ]))
                    .AddGlobal(HealthDensityRule)
            },
            {
                HistoryRecordDefOf.MechlinkInstalled,
                new HistoryBackfillDefinition(HistoryRecordDefOf.MechlinkInstalled)
                    .AddHard(
                        new MinimumAgeRule(13f),
                        new MaximumCountRule(1),
                        new LogicalGateRule((_, _) => ModsConfig.BiotechActive))
                    .AddSoft(new AgeCurveSoftRule([
                        new CurvePoint(13f, 0.01f),
                        new CurvePoint(16f, 0.08f),
                        new CurvePoint(20f, 0.3f),
                        new CurvePoint(28f, 1f),
                        new CurvePoint(45f, 1.1f),
                        new CurvePoint(70f, 0.4f)
                    ]))
            }
        };
    }
}
