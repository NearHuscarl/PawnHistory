using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class Surgery_RemovePartRecorder : SurgeryRecorder
{
    public override void Register()
    {
        GameEventBus.Subscribe<SurgeryRemoveBodyPartEvent>(e =>
        {
            if (!ShouldRecord(e.Patient))
                return;

            if (e.Outcome.failure)
            {
                var botched = HistoryRecordDefOf.BodyPartRemoved.Description("BotchedSurgery", e.Patient)
                    .AddRule("Part", e.Part)
                    .AddConstant("intent", e.Intent)
                    .Resolve()
                    .ToLower();
                HandleBotchSurgeryEvent(e, botched);
            }
            else
                HandleBodyPartRemovedEvent(e);
        });
    }

    private void HandleBodyPartRemovedEvent(SurgeryRemoveBodyPartEvent e)
    {
        var recordDef = HistoryRecordDefOf.BodyPartRemoved;
        var desc = recordDef.Description("bodyPartRemoved", e.Patient)
            .AddRule("Doctor", e.Doctor)
            .AddRule("Part", e.Part.Label.Colorize(HediffDefOf.MissingBodyPart.defaultLabelColor))
            .AddRule("BadHediff", e.BadHediff?.LabelNounFull())
            .AddConstant("intent", e.Intent)
            .Resolve();
        AddRecord(recordDef, e.Patient, desc, [e.Doctor]);
    }

    public void TestHarvest(TestScenario scenario)
    {
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .FullHeal()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, RecipeDefOf.RemoveBodyPart, BodyPartDefOf.Lung)
            .CreateSingle();
    }

    public void TestAmputate(TestScenario scenario)
    {
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
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
            .DoSurgery(patient, RecipeDefOf.RemoveBodyPart, BodyPartDefOf.Leg)
            .CreateSingle();
    }

    public void TestFail(TestScenario scenario)
    {
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .FullHeal()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor(isBadDoctor: true)
            .DoSurgery(patient, RecipeDefOf.RemoveBodyPart, BodyPartDefOf.Lung, instant: true)
            .CreateSingle();
    }
}