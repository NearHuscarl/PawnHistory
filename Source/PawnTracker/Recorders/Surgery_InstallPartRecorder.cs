using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class Surgery_InstallPartRecorder : SurgeryRecorder
{
    public override void Register()
    {
        GameEventBus.Subscribe<SurgeryInstallNaturalPartEvent>(e =>
        {
            if (e.Outcome.failure)
            {
                var botched = HistoryRecordDefOf.BodyPartInstalled.Description(e.Patient)
                    .AddRule("AddedPart", e.Part, addSubsymbols: true)
                    .Resolve("BotchedSurgery")
                    .ToLower();
                HandleBotchSurgeryEvent(e, botched);
            }
            else
                HandleBodyPartInstalledEvent(e);
        });
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

    private void HandleBodyPartInstalledEvent(SurgeryInstallNaturalPartEvent e)
    {
        if (!ShouldRecord(e.Patient))
            return;

        var recordDef = HistoryRecordDefOf.BodyPartInstalled;
        var desc = recordDef.Description(e.Patient)
            .AddRule("Doctor", e.Doctor)
            .AddRule("RemovedPart", e.Part)
            .AddRule("RemovedPart", e.HediffToRemove, replaceIfExist: true)
            .AddRule("BadHediff", e.BadHediff?.LabelNounFull())
            .AddConstant("type", GetSurgeryType(e))
            .AddRule("AddedPart", e.Part, addSubsymbols: true)
            .Resolve();
        AddRecord(recordDef, e.Patient, desc, [e.Doctor]);
    }

    public void TestInstall(TestScenario scenario)
    {
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing("Lung", 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .AddHediff(HediffDefOf.MissingBodyPart, BodyPartDefOf.Lung)
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, DefDatabase<RecipeDef>.GetNamed("InstallNaturalLung"), BodyPartDefOf.Lung)
            .CreateSingle();
    }

    public void TestReplace(TestScenario scenario)
    {
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing("Heart", 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .AddHediff("SimpleProstheticHeart", BodyPartDefOf.Heart)
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, DefDatabase<RecipeDef>.GetNamed("InstallNaturalHeart"), BodyPartDefOf.Heart)
            .CreateSingle();
    }

    public void TestFix(TestScenario scenario)
    {
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing("Heart", 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .AddHediff("HeartArteryBlockage", BodyPartDefOf.Heart)
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, DefDatabase<RecipeDef>.GetNamed("InstallNaturalHeart"), BodyPartDefOf.Heart)
            .CreateSingle();
    }

    public void TestFail(TestScenario scenario)
    {
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing("Kidney", 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor(isBadDoctor: true)
            .DoSurgery(patient, DefDatabase<RecipeDef>.GetNamed("InstallNaturalKidney"), DefDatabase<BodyPartDef>.GetNamed("Kidney"), instant: true)
            .CreateSingle();
    }
}