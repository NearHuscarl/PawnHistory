using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class GrowthMomentRecorder : RecorderBase<GrowthMomentEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<GrowthMomentEvent>(CreateRecord);
    }

    public override void CreateRecord(GrowthMomentEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var passions = e.SkillsWithNewPassion ?? [];
        if (e.Trait == null && passions.Count == 0)
            return;

        var recordDef = HistoryRecordDefOf.GrowthMoment;
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddRule("Trait", e.Trait?.CurrentData.label.Colorize(ColoredText.TipSectionTitleColor))
            .AddRule("SkillList", LangUtility.FormatList(passions, s => s.skillLabel.Colorize(ColoredText.SubtleGrayColor)))
            .AddConstant("hasTrait", e.Trait != null)
            .AddConstant("hasNewPassion", e.SkillsWithNewPassion.Count > 0)
            .AddConstant("passionCount", e.SkillsWithNewPassion.Count)
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc);
    }

    [RequiresBiotech]
    public void TestTraitOnly(TestScenario scenario)
    {
        var child = CreateGrowthMomentPawn(scenario, growthTier: 0, age: 6);

        scenario.GrowthMomentLetter().TraitIndex(0).Execute();

        Expect.That(child).ToHaveHistoryRecord(HistoryRecordDefOf.GrowthMoment, "At biological age [PAWN_age], [PAWN] experienced a growth moment and gained the [Trait] trait.");
    }

    [RequiresBiotech]
    public void TestPassionOnly(TestScenario scenario)
    {
        var child = CreateGrowthMomentPawn(scenario, growthTier: 4, age: 6);

        scenario.GrowthMomentLetter().PassionIndices([0]).Execute();

        Expect.That(child).ToHaveHistoryRecord(HistoryRecordDefOf.GrowthMoment, "At biological age [PAWN_age], [PAWN] experienced a growth moment and developed a passion for [SkillList].");
    }

    [RequiresBiotech]
    public void TestTraitAndPassion(TestScenario scenario)
    {
        var child = CreateGrowthMomentPawn(scenario, growthTier: 4, age: 6);

        scenario.GrowthMomentLetter().TraitIndex(0).PassionIndices([0]).Execute();

        Expect.That(child).ToHaveHistoryRecord(HistoryRecordDefOf.GrowthMoment, "At biological age [PAWN_age], [PAWN] experienced a growth moment, gaining the [Trait] trait and developing a passion for [SkillList]");
    }

    [RequiresBiotech]
    public void TestTraitAndPassions(TestScenario scenario)
    {
        var child = CreateGrowthMomentPawn(scenario, growthTier: 7, age: 9);

        scenario.GrowthMomentLetter().TraitIndex(0).PassionIndices([0, 1]).Execute();

        Expect.That(child).ToHaveHistoryRecord(HistoryRecordDefOf.GrowthMoment, "At biological age [PAWN_age], [PAWN] experienced a growth moment, gaining the [Trait] trait and developing passions for [SkillList]");
    }

    private static Pawn CreateGrowthMomentPawn(TestScenario scenario, int growthTier, int age)
    {
        return scenario.Pawn()
            .Colonist()
            .SetAge(age)
            .Do(p => p.skills.skills.ForEach(s => s.passion = Passion.None))
            .SetGrowthTier(growthTier)
            .ForceBirthday()
            .CreateSingle();
    }
}
