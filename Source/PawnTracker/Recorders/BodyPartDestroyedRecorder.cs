using PawnHistory.Source.Helper;
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
            .EquipWeapon("Weapon_GrenadeFrag", (_, i) => i % 2 == 0)
            .WeakenParts(nonVitalParts, oneSide: true)
            .Execute();

        scenario.SpeedUp();
    }

    // hasInstigator==,hasDmgSource==
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
            .Execute();

        var tickStart = Find.TickManager.TicksGame;

        scenario.SpeedUp();
        scenario.RunUntil(() => Find.TickManager.TicksGame - tickStart > 2500, () =>
        {
            foreach (var pawn in pawns)
            {
                if (pawn == null) continue;
                FireUtility.TryAttachFire(pawn, 1.75f, null);
            }
        }, interval: 100);
    }
}
