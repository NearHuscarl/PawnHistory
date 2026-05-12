using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.HistoryBackfill;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class WeaponBondedRecorder : RecorderBase<WeaponBondedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<WeaponBondedEvent>(CreateRecord);
    }

    internal override IEnumerable<HistoryBackfillDefinition> GetBackfillDefinitions()
    {
        if (!ModsConfig.RoyaltyActive)
            yield break;

        yield return new HistoryBackfillDefinition(HistoryRecordDefOf.WeaponBonded)
            .AddHard(
                new MinimumAgeRule(13f),
                new MaximumCountRule(1),
                new LogicalGateRule((_, _) => ModsConfig.RoyaltyActive))
            .AddSoft(new AgeCurveSoftRule([
                new CurvePoint(13f, 0.01f),
                new CurvePoint(18f, 0.12f),
                new CurvePoint(24f, 0.45f),
                new CurvePoint(32f, 1f),
                new CurvePoint(48f, 1.1f),
                new CurvePoint(70f, 0.35f)
            ]));
    }

    public override void CreateRecord(WeaponBondedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var recordDef = HistoryRecordDefOf.WeaponBonded;
        var desc = recordDef.Description(e.Pawn)
            .AddRule("Wpn", e.Weapon.Label.Colorize(ColoredText.SubtleGrayColor), addSubsymbols: true)
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc, [e.Weapon]);
    }

    [RequiresRoyalty]
    public void Test(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var weapon = scenario.Thing(Extra.ThingDefOf.MeleeWeapon_MonoSwordBladelink).CreateSingle<ThingWithComps>();

        weapon.GetComp<CompBladelinkWeapon>().CodeFor(pawn);

        Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.WeaponBonded,
            Description = "[PAWN] formed a persona bond with [Weapon].",
            Concerns = [weapon],
        });
    }
}
