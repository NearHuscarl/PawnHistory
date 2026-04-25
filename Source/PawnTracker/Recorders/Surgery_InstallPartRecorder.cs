using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class Surgery_InstallPartRecorder : SurgeryRecorder<SurgeryInstallNaturalPartEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<SurgeryInstallNaturalPartEvent>(CreateRecord);
    }

    enum SurgeryType
    {
        Install,
        Replace,
        Fix,
    };

    private SurgeryType GetSurgeryType(SurgeryInstallNaturalPartEvent e)
    {
        if (e.HediffToRemove is Hediff_MissingPart)
            return SurgeryType.Install;
        else if (e.BadHediff != null)
            return SurgeryType.Fix;
        return SurgeryType.Replace;
    }

    public override void CreateRecord(SurgeryInstallNaturalPartEvent e)
    {
        if (!ShouldRecord(e.Patient))
            return;

        if (e.Outcome.failure)
        {
            var botched = HistoryRecordDefOf.BodyPartInstalled.Description(e.Patient)
                .AddRule("AddedPart", e.Part, addSubsymbols: true)
                .Resolve("BotchedSurgery")
                .ToLower();
            RecordBotchedSurgery(e, botched);
            return;
        }

        var recordDef = HistoryRecordDefOf.BodyPartInstalled;
        var desc = recordDef.Description(e.Patient)
            .AddRule("Doctor", e.Doctor)
            .AddRule("RemovedPart", e.Part)
            .AddRule("RemovedPart", e.HediffToRemove, replaceIfExist: true)
            .AddRule("BadHediff", e.BadHediff?.LabelNounInBracket())
            .AddConstant("type", GetSurgeryType(e))
            .AddRule("AddedPart", e.Part, addSubsymbols: true)
            .Resolve();
        AddRecord(recordDef, e.Patient, desc, [e.Doctor]);
    }

    [SkipTest]
    public void TestInstall(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing(DefLookup.Thing.Lung, 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .AddHediff(HediffDefOf.MissingBodyPart, BodyPartDefOf.Lung)
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, DefLookup.Recipe.InstallNaturalLung, BodyPartDefOf.Lung)
            .CreateSingle();
    }

    [SkipTest]
    public void TestReplace(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing(DefLookup.Thing.Heart, 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .AddHediff(DefLookup.Hediff.SimpleProstheticHeart, BodyPartDefOf.Heart)
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, DefLookup.Recipe.InstallNaturalHeart, BodyPartDefOf.Heart)
            .CreateSingle();
    }

    [SkipTest]
    public void TestFix(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing(DefLookup.Thing.Heart, 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .AddHediff(DefLookup.Hediff.HeartArteryBlockage, BodyPartDefOf.Heart)
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, DefLookup.Recipe.InstallNaturalHeart, BodyPartDefOf.Heart)
            .CreateSingle();
    }

    [SkipTest]
    public void TestFail(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing(DefLookup.Thing.Kidney, 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor(isBadDoctor: true)
            .DoSurgery(patient, DefLookup.Recipe.InstallNaturalKidney, DefLookup.BodyPart.Kidney, instant: true)
            .CreateSingle();
    }
}
