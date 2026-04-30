using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class BodyPartDestroyedRecorder : RecorderBase<HediffAddedEvent>
{
    public override void Register()
    {
        // Very important mental model:
        // - Normal: PreAddHediff(hediff) > PostAddHediff(hediff)
        // - If a hediff causes missing part: PreAddHediff(SurgicalCut) > PreAddHediff(MissingBodyPart) > PostAddHediff(MissingBodyPart) > PostAddHediff(SurgicalCut)
        // Reference: Pawn_HealthTracker.AddHediff() > HediffSet.AddDirect()
        GameEventBus.Subscribe<HediffAddedEvent>(e =>
        {
            if (e.Part == null)
                return;

            if (e.Hediff.def == HediffDefOf.MissingBodyPart)
            {
                // missing vital body parts will make a pawn die, this is handled by CasualtyRecorder instead.
                if (e.Pawn.Dead)
                    return;

                // handled by BodyPartRemovedRecorder
                if (e.Dinfo?.Def == DamageDefOf.SurgicalCut)
                    return;
                
                CreateRecord(e);
            }
        });
    }

    // Must be called in postfix because hediff.label does not exist in prefix.
    public override void CreateRecord(HediffAddedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var (pawn, hediff, part, dinfo) = e;
        var instigator = dinfo?.Instigator as Pawn;
        var dmgSource = dinfo?.GetDamageSource();
        var recordDef = HistoryRecordDefOf.BodyPartDestroyed;
        var desc = recordDef.Description(pawn)
            .AddRule("Part", part.Label.Colorize(hediff.LabelColor))
            .AddRule("Destroyed", hediff) // <destroyedLabel>
            .AddRule("Instigator", instigator)
            .AddConstant("hasInstigator", instigator != null)
            .AddRule("DmgSource", dmgSource)
            .AddConstant("hasDmgSource", !dmgSource.NullOrEmpty())
            .Resolve();

        AddRecord(recordDef, pawn, desc, [instigator]);
    }

    // hasInstigator==true,hasDmgSource==true
    [SkipTest]
    public void Test(TestScenario scenario)
    {
        HashSet<BodyPartDef> nonVitalParts =
        [
            BodyPartDefOf.Hand,
            BodyPartDefOf.Eye,
            BodyPartDefOf.Shoulder,
            Extra.BodyPartDefOf.Nose,
            Extra.BodyPartDefOf.Ear,
            Extra.BodyPartDefOf.Foot,
        ];

        var pawns1 = scenario.Incident(IncidentDefOf.RaidEnemy)
            .Point(350)
            .RaidArrivalMode(PawnsArrivalModeDefOf.EdgeWalkIn)
            .Execute();

        var pawns2 = scenario.RaidFriendly()
            .RaidArrivalMode(PawnsArrivalModeDefOf.EdgeWalkIn)
            .Point(400)
            .Execute();

        scenario.Pawn(pawns1.Concat(pawns2))
            .AddHediff(Extra.HediffDefOf.Painstopper, Extra.BodyPartDefOf.Brain)
            .AddHediff(Extra.HediffDefOf.GoJuiceHigh, Extra.BodyPartDefOf.Brain)
            .EquipWeapon(Extra.ThingDefOf.Weapon_GrenadeFrag, (_, i) => i % 2 == 0)
            .WeakenParts(nonVitalParts, oneSide: true)
            .Execute();

        scenario.SpeedUp();
    }

    // hasInstigator==,hasDmgSource==
    [SkipTest]
    public void TestBurn(TestScenario scenario)
    {
        HashSet<BodyPartDef> nonVitalParts =
        [
            BodyPartDefOf.Eye,
            BodyPartDefOf.Shoulder,
            BodyPartDefOf.Leg,
            Extra.BodyPartDefOf.Nose,
            Extra.BodyPartDefOf.Ear,
        ];

        var pawns = scenario.RaidFriendly()
            .RaidArrivalMode(PawnsArrivalModeDefOf.EdgeWalkIn)
            .Point(1000)
            .Execute();

        scenario.Pawn(pawns)
            .AddHediff(Extra.HediffDefOf.Painstopper, Extra.BodyPartDefOf.Brain)
            .AddHediff(Extra.HediffDefOf.GoJuiceHigh, Extra.BodyPartDefOf.Brain)
            .WeakenParts(nonVitalParts, true)
            .Execute();

        scenario.SpeedUp();
        scenario.Loop(_ =>
        {
            foreach (var pawn in pawns)
            {
                pawn.TryAttachFire(1.75f, null);
            }
        }, interval: 100);
    }
}
