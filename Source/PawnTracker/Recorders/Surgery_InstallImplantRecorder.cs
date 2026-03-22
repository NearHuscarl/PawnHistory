using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class Surgery_InstallImplantRecorder : SurgeryRecorder
{
    public override void Register()
    {
        GameEventBus.Subscribe<SurgeryInstallImplantEvent>(e =>
        {
            if (e.Outcome.failure)
            {
                var h = e.HediffToAdd.label.ToLowerInvariant().Colorize(e.HediffToAdd.defaultLabelColor);
                HandleBotchSurgeryEvent(e, $"{h} implantation");
            }
            else
                HandleBodyPartImplantedEvent(e);
        });
    }

    private void HandleBodyPartImplantedEvent(SurgeryInstallImplantEvent e)
    {
        if (!ShouldRecord(e.Patient))
            return;

        var recordDef = HistoryRecordDefOf.BodyPartImplanted;
        var desc = recordDef.ResolveDescription("bodyPartImplanted", e.Patient)
            .AddRule("Doctor", e.Doctor)
            .AddRule("ImplantHediff", e.HediffToAdd, addSubsymbols: true)
            .AddRule("EnhancedPart", e.Part.LabelShort)
            .Resolve();
        AddRecord(recordDef, e.Patient, desc, [e.Doctor]);
    }

    public override void Test(TestScenario scenario)
    {
        var beds = new List<Building_Bed>();
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2, beds)
            .WithThing("Joywire", 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, beds[0], DefDatabase<RecipeDef>.GetNamed("InstallJoywire"), DefDatabase<BodyPartDef>.GetNamed("Brain"))
            .CreateSingle();
    }

    public void TestFail(TestScenario scenario)
    {
        var beds = new List<Building_Bed>();
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2, beds)
            .WithThing("Joywire", 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor(isBadDoctor: true)
            .AddHediff(HediffDefOf.MissingBodyPart, BodyPartDefOf.Arm, partIndex: 0)
            .AddHediff(HediffDefOf.MissingBodyPart, BodyPartDefOf.Arm, partIndex: 1)
            .AddHediff(HediffDefOf.MissingBodyPart, BodyPartDefOf.Eye, partIndex: 0)
            .AddHediff(HediffDefOf.MissingBodyPart, BodyPartDefOf.Eye, partIndex: 1)
            .AddHediff("SmokeleafHigh", BodyPartDefOf.Torso)
            .DoSurgery(patient, beds[0], DefDatabase<RecipeDef>.GetNamed("InstallJoywire"), DefDatabase<BodyPartDef>.GetNamed("Brain"), instant: true)
            .CreateSingle();
    }
}