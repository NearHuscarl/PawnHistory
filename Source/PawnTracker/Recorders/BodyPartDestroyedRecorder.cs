using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class BodyPartDestroyedRecorder : RecorderBase
{
    public override void Register()
    {
        // Very important mental model:
        // - Normal: PreAddHediff(hediff) > PostAddHediff(hediff)
        // - If a hediff causes missing part: PreAddHediff(SurgicalCut) > PreAddHediff(MissingBodyPart) > PostAddHediff(MissingBodyPart) > PostAddHediff(SurgicalCut)
        // Reference: Pawn_HealthTracker.AddHediff() > HediffSet.AddDirect()
        GameEventBus.Subscribe<HediffAddedEvent>(e =>
        {
            var pawn = e.Pawn;
            var hediff = e.Hediff;
            var part = e.Part;
            var dinfo = e.Dinfo;

            if (!ShouldRecord(pawn))
                return;

            if (part == null)
                return;

            if (hediff.def == HediffDefOf.MissingBodyPart)
            {
                // missing vital body parts will make a pawn die, this is handled by CasualtyRecorder instead.
                if (pawn.Dead)
                    return;

                // handled by BodyPartRemovedRecorder
                if (dinfo?.Def == DamageDefOf.SurgicalCut)
                    return;
                
                HandleDestroyPartEvent(pawn, hediff, part, dinfo);
            }
        });
    }

    // Must be called in postfix because hediff.label does not exist in prefix.
    private void HandleDestroyPartEvent(Pawn pawn, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo)
    {
        var instigator = dinfo?.Instigator as Pawn;
        var weapon = dinfo?.Weapon?.race != null ? dinfo?.Tool?.label /* body part like fist/teeth */ : dinfo?.Weapon?.label;
        var recordDef = HistoryRecordDefOf.BodyPartDestroyed;
        var descBuilder = recordDef.ResolveDescription("bodyPartDestroyed", pawn)
            .AddRule("PART", part.Label.Colorize(hediff.LabelColor))
            .AddRule("Destroyed", hediff) // <destroyedLabel>
            .AddRule("WEAPON", weapon)
            .AddConstantIf(weapon != null, "hasWeapon", "true");

        if (dinfo?.Instigator is Pawn)
        {
            descBuilder
                .AddRule("INSTIGATOR", instigator)
                .AddConstant("hasInstigator", "true");
        }

        AddRecord(recordDef, pawn, descBuilder.Resolve(), [instigator]);
    }

    // hasInstigator==true,hasWeapon==true
    public override void Test(TestScenario scenario)
    {
        HashSet<BodyPartDef> nonVitalParts =
        [
            BodyPartDefOf.Hand,
            BodyPartDefOf.Eye,
            BodyPartDefOf.Shoulder,
            DefDatabase<BodyPartDef>.GetNamed("Nose"),
            DefDatabase<BodyPartDef>.GetNamed("Ear"),
            DefDatabase<BodyPartDef>.GetNamed("Foot"),
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
            .AddHediff("Painstopper", "Brain")
            .AddHediff("GoJuiceHigh", "Brain")
            .WeakenParts(nonVitalParts, oneSide: true)
            .Create();
    }

    // hasInstigator==,hasWeapon==
    public void TestBurn(TestScenario scenario)
    {
        HashSet<BodyPartDef> nonVitalParts =
        [
            BodyPartDefOf.Eye,
            BodyPartDefOf.Shoulder,
            BodyPartDefOf.Leg,
            DefDatabase<BodyPartDef>.GetNamed("Nose"),
            DefDatabase<BodyPartDef>.GetNamed("Ear"),
        ];

        var pawns = scenario.RaidFriendly()
            .RaidArrivalMode(PawnsArrivalModeDefOf.EdgeWalkIn)
            .Point(1000)
            .Execute();

        scenario.Pawn(pawns)
            .AddHediff("Painstopper", "Brain")
            .AddHediff("GoJuiceHigh", "Brain")
            .WeakenParts(nonVitalParts, true)
            .Create();

        Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;

        var interval = TickDelayManager.Interval(100, () =>
        {
            foreach (var pawn in pawns)
                FireUtility.TryAttachFire(pawn, 1.75f, null);
        });

        TickDelayManager.Delay(2500, () => TickDelayManager.Cancel(interval));
    }
}
