using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class Surgery_RemoveBodyPartRecorder : SurgeryRecorder
{
    public override void Register()
    {
        GameEventBus.Subscribe<SurgeryRemoveBodyPartEvent>(e =>
        {
            if (!ShouldRecord(e.Patient))
                return;

            if (e.Outcome.failure)
                HandleBotchSurgeryEvent(e, e.Intent.ToString().ToLowerInvariant());
            else
                HandleBodyPartRemovedEvent(e);
        });
    }

    private void HandleBodyPartRemovedEvent(SurgeryRemoveBodyPartEvent e)
    {
        var recordDef = HistoryRecordDefOf.BodyPartRemoved;
        var desc = recordDef.ResolveDescription("bodyPartRemoved", e.Patient)
            .AddRule("Doctor", e.Doctor)
            .AddRule("Part", e.Part.Label.Colorize(HediffDefOf.MissingBodyPart.defaultLabelColor))
            .AddRule("BadHediff", e.BadHediff?.LabelNounFull())
            .AddConstant("intent", e.Intent)
            .Resolve();
        AddRecord(recordDef, e.Patient, desc, [e.Doctor]);
    }

    public void TestFail(TestScenario scenario)
    {
        var beds = new List<Building_Bed>();
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2, beds)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .FullHeal()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor(isBadDoctor: true)
            .AddHediff(HediffDefOf.MissingBodyPart, BodyPartDefOf.Arm, partIndex: 0)
            .AddHediff(HediffDefOf.MissingBodyPart, BodyPartDefOf.Arm, partIndex: 1)
            .AddHediff(HediffDefOf.MissingBodyPart, BodyPartDefOf.Eye, partIndex: 0)
            .AddHediff(HediffDefOf.MissingBodyPart, BodyPartDefOf.Eye, partIndex: 1)
            .AddHediff("SmokeleafHigh", BodyPartDefOf.Torso)
            .DoSurgery(patient, beds[0], RecipeDefOf.RemoveBodyPart, BodyPartDefOf.Lung, instant: true)
            .CreateSingle();
    }

    public void TestHarvest(TestScenario scenario)
    {
        var beds = new List<Building_Bed>();
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2, beds)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .FullHeal()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, beds[0], RecipeDefOf.RemoveBodyPart, BodyPartDefOf.Lung)
            .CreateSingle();
    }

    public void TestAmputate(TestScenario scenario)
    {
        var beds = new List<Building_Bed>();
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2, beds)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .FullHeal()
            .AddHediff(HediffDefOf.WoundInfection, BodyPartDefOf.Leg, h => h.Severity = 0.8f)
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, beds[0], RecipeDefOf.RemoveBodyPart, BodyPartDefOf.Leg)
            .CreateSingle();
    }
}