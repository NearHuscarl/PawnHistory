using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class Surgery_ModPartRecorder : SurgeryRecorder<SurgeryInstallArtificialPartEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<SurgeryInstallArtificialPartEvent>(CreateRecord);
    }

    enum SurgeryType
    {
        Install,
        Replace,
        Fix,
    };

    private SurgeryType GetSurgeryType(SurgeryInstallArtificialPartEvent e)
    {
        if (e.HediffToRemove is Hediff_MissingPart)
            return SurgeryType.Install;
        else if (e.BadHediff != null)
            return SurgeryType.Fix;
        return SurgeryType.Replace;
    }

    public override void CreateRecord(SurgeryInstallArtificialPartEvent e)
    {
        if (!ShouldRecord(e.Patient))
            return;

        if (e.Outcome.failure)
        {
            var botched = HistoryRecordDefOf.BodyPartModded.Description(e.Patient)
                .AddRule("AddedHediff", e.Part, addSubsymbols: true)
                .Resolve("BotchedSurgery")
                .ToLower();
            RecordBotchedSurgery(e, botched);
            return;
        }

        var recordDef = HistoryRecordDefOf.BodyPartModded;
        var desc = recordDef.Description(e.Patient)
            .IncludePawnGrammar()
            .AddRule("Doctor", e.Doctor)
            .AddRule("RemovedPart", e.Part)
            .AddRule("RemovedPart", e.HediffToRemove, replaceIfExist: true)
            .AddRule("BadHediff", e.BadHediff?.LabelNounInBracket())
            .AddRule("AddedHediff", e.HediffToAdd, addSubsymbols: true)
            .AddConstant("type", GetSurgeryType(e))
            .AddConstant("isViolation", e.IsViolation)
            .Resolve();
        AddRecord(recordDef, e.Patient, desc, [e.Doctor]);
    }

    [SkipTest]
    public void TestInstall(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing(Extra.ThingDefOf.BionicArm, 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .AddHediff(HediffDefOf.MissingBodyPart, BodyPartDefOf.Shoulder)
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, Extra.RecipeDefOf.InstallBionicArm, BodyPartDefOf.Shoulder)
            .CreateSingle();
    }

    [SkipTest]
    public void TestReplace(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing(Extra.ThingDefOf.BionicHeart, 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, Extra.RecipeDefOf.InstallBionicHeart, BodyPartDefOf.Heart)
            .CreateSingle();
    }

    [SkipTest]
    public void TestViolation(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing(Extra.ThingDefOf.SimpleProstheticHeart, 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .AddHediff(Extra.HediffDefOf.BionicHeart, BodyPartDefOf.Heart)
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, Extra.RecipeDefOf.InstallSimpleProstheticHeart, BodyPartDefOf.Heart)
            .CreateSingle();
    }

    [SkipTest]
    public void TestFix(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing(Extra.ThingDefOf.BionicHeart, 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .AddHediff(Extra.HediffDefOf.HeartArteryBlockage, BodyPartDefOf.Heart)
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, Extra.RecipeDefOf.InstallBionicHeart, BodyPartDefOf.Heart)
            .CreateSingle();
    }

    [SkipTest]
    public void TestFail(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing(Extra.ThingDefOf.BionicHeart, 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor(isBadDoctor: true)
            .DoSurgery(patient, Extra.RecipeDefOf.InstallBionicHeart, BodyPartDefOf.Heart, instant: true)
            .CreateSingle();
    }
}
