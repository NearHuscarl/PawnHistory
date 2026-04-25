using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class Surgery_ImplantRecorder : SurgeryRecorder<SurgeryInstallImplantEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<SurgeryInstallImplantEvent>(CreateRecord);
    }

    public override void CreateRecord(SurgeryInstallImplantEvent e)
    {
        if (!ShouldRecord(e.Patient))
            return;

        if (e.Outcome.failure)
        {
            var botched = HistoryRecordDefOf.BodyPartImplanted.Description(e.Patient)
                .AddRule("ImplantHediff", e.HediffToAdd, addSubsymbols: true)
                .Resolve("BotchedSurgery")
                .ToLower();
            RecordBotchedSurgery(e, botched);
            return;
        }

        var recordDef = HistoryRecordDefOf.BodyPartImplanted;
        var desc = recordDef.Description(e.Patient)
            .AddRule("Doctor", e.Doctor)
            .AddRule("ImplantHediff", e.HediffToAdd, addSubsymbols: true)
            .AddRule("EnhancedPart", e.Part)
            .Resolve();
        AddRecord(recordDef, e.Patient, desc, [e.Doctor]);
    }

    [SkipTest]
    public void Test(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing(DefLookup.Thing.Joywire, 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, DefLookup.Recipe.InstallJoywire, DefLookup.BodyPart.Brain)
            .CreateSingle();
    }

    [SkipTest]
    public void TestFail(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing(DefLookup.Thing.Joywire, 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor(isBadDoctor: true)
            .DoSurgery(patient, DefLookup.Recipe.InstallJoywire, DefLookup.BodyPart.Brain, instant: true)
            .CreateSingle();
    }
}
