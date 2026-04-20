using PawnHistory.Source.PawnTracker.Events;
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

    public override void CreateRecord(WeaponBondedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var recordDef = HistoryRecordDefOf.WeaponBonded;
        var desc = recordDef.Description(e.Pawn)
            .AddRule("Wpn", e.Weapon.Label.Colorize(ColoredText.SubtleGrayColor), addSubsymbols: true)
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc, [e.Pawn, e.Weapon]);
    }

    [RequiresRoyalty]
    public void Test(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var weapon = scenario.Thing(DefLookup.Thing.MeleeWeapon_MonoSwordBladelink).Create<ThingWithComps>();

        weapon.GetComp<CompBladelinkWeapon>().CodeFor(pawn);

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] formed a persona bond with [Weapon].", HistoryRecordDefOf.WeaponBonded);
        Expect.That(pawn).ToHaveHistoryRecordConcern(weapon, HistoryRecordDefOf.WeaponBonded);
    }
}
