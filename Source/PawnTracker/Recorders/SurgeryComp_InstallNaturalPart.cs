using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using PawnHistory.Source.PawnTracker.Test.Mocks;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class SurgeryComp_InstallNaturalPart : SurgeryComp
{
    public override bool Match(BuildInput input) => input.Event.Data is SurgeryInstallNaturalPartData;

    public override HistoryRecordDef RecordDef(BuildInput input) => HistoryRecordDefOf.BodyPartInstalled;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var e = input.Event;
        var data = (SurgeryInstallNaturalPartData)e.Data;
        return builder
            .AddRule("RemovedPart", e.Part)
            .AddRule("RemovedPart", data.HediffToRemove, replaceIfExist: true)
            .AddRule("BadHediff", data.BadHediff?.LabelNounInBracket())
            .AddConstant("type", GetSurgeryType(data))
            .AddRule("AddedPart", e.Part, addSubsymbols: true);
    }

    public override HistoryDescriptionBuilder BuildBotchedGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var e = input.Event;
        return builder.AddRule("AddedPart", e.Part, addSubsymbols: true);
    }
    
    private enum SurgeryType
    {
        Install,
        Replace,
        Fix,
    }

    private static SurgeryType GetSurgeryType(SurgeryInstallNaturalPartData data)
    {
        if (data.HediffToRemove is Hediff_MissingPart)
            return SurgeryType.Install;
        if (data.BadHediff != null)
            return SurgeryType.Fix;
        return SurgeryType.Replace;
    }

    public void TestInstall(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.InstallNaturalLung,
            BodyPartDefOf.Lung,
            patient => patient.AddHediff(HediffDefOf.MissingBodyPart, BodyPartDefOf.Lung));

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BodyPartInstalled,
            Description = "[Doctor] installed a left lung on [PAWN].",
            Concerns = [doctor],
        });
    }

    public void TestReplace(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.InstallNaturalHeart,
            BodyPartDefOf.Heart,
            patient => patient.AddHediff(Extra.HediffDefOf.SimpleProstheticHeart, BodyPartDefOf.Heart));

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BodyPartInstalled,
            Description = "[Doctor] replaced [PAWN]'s prosthetic heart with a heart.",
            Concerns = [doctor],
        });
    }

    public void TestFix(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.InstallNaturalHeart,
            BodyPartDefOf.Heart,
            patient => patient.AddHediff(Extra.HediffDefOf.HeartArteryBlockage, BodyPartDefOf.Heart));

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BodyPartInstalled,
            Description = "[Doctor] installed a healthy heart on [PAWN] to treat an artery blockage (minor).",
            Concerns = [doctor],
        });
    }

    public void TestFail(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.InstallNaturalKidney,
            Extra.BodyPartDefOf.Kidney,
            surgeryOutcome: SurgeryOutcomes.MinorFailure);

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BotchedSurgery,
            Description = "[Doctor] slightly botched the installation of a left kidney on [PAWN], causing [NewInjuries].",
            Concerns = [doctor],
        });
    }
}
