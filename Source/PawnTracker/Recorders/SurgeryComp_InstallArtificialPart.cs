using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using PawnHistory.Source.PawnTracker.Test.Mocks;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class SurgeryComp_InstallArtificialPart : SurgeryComp
{
    public override bool Match(BuildInput input) => input.Event is SurgeryInstallArtificialPartEvent;

    public override HistoryRecordDef RecordDef(BuildInput input) => HistoryRecordDefOf.BodyPartModded;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var e = (SurgeryInstallArtificialPartEvent)input.Event;
        return builder
            .AddRule("RemovedPart", e.Part)
            .AddRule("RemovedPart", e.HediffToRemove, replaceIfExist: true)
            .AddRule("BadHediff", e.BadHediff?.LabelNounInBracket())
            .AddRule("AddedHediff", e.HediffToAdd, addSubsymbols: true)
            .AddConstant("type", GetSurgeryType(e))
            .AddConstant("isViolation", e.IsViolation);
    }

    public override HistoryDescriptionBuilder BuildBotchedGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var e = (SurgeryInstallArtificialPartEvent)input.Event;
        return builder.AddRule("AddedHediff", e.HediffToAdd, addSubsymbols: true);
    }

    private enum SurgeryType
    {
        Install,
        Replace,
        Fix,
    }

    private static SurgeryType GetSurgeryType(SurgeryInstallArtificialPartEvent e)
    {
        if (e.HediffToRemove is Hediff_MissingPart)
            return SurgeryType.Install;
        if (e.BadHediff != null)
            return SurgeryType.Fix;
        return SurgeryType.Replace;
    }

    public void TestInstall(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.InstallBionicArm,
            BodyPartDefOf.Shoulder,
            p => p.AddHediff(HediffDefOf.MissingBodyPart, BodyPartDefOf.Shoulder));
        var (patient2, doctor2) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.InstallDenture,
            Extra.BodyPartDefOf.Jaw,
            p => p.AddHediff(HediffDefOf.MissingBodyPart, Extra.BodyPartDefOf.Jaw));
        
        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BodyPartModded,
            Description = "[Doctor] installed a bionic arm on [PAWN]'s left shoulder.",
            Concerns = [doctor],
        });
        Expect.That(patient2).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BodyPartModded,
            Description = "[Doctor] installed a denture on [PAWN].",
            Concerns = [doctor2],
        });
    }

    public void TestReplace(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.InstallArchotechArm,
            BodyPartDefOf.Shoulder);
        var (patient2, doctor2) = SurgeryRecorder.DoSurgery(scenario, Extra.RecipeDefOf.InstallBionicHeart, BodyPartDefOf.Heart,
            p => p.AddHediff(Extra.HediffDefOf.SimpleProstheticHeart, BodyPartDefOf.Heart));
        
        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BodyPartModded,
            Description = "[Doctor] replaced [PAWN]'s left shoulder with an archotech arm.", // replace [Part] with [Hediff]
            Concerns = [doctor],
        });
        Expect.That(patient2).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BodyPartModded,
            Description = "[Doctor] replaced [PAWN]'s prosthetic heart with a bionic heart.", // replace [Hediff] with [Hediff]
            Concerns = [doctor2],
        });
    }

    public void TestViolation(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.InstallSimpleProstheticHeart,
            BodyPartDefOf.Heart,
            patient => patient.AddHediff(Extra.HediffDefOf.BionicHeart, BodyPartDefOf.Heart));

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BodyPartModded,
            Description = "[Doctor] replaced [PAWN]'s bionic heart with a prosthetic heart, violating [His] body.",
            Concerns = [doctor],
        });
    }

    public void TestFix(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.InstallBionicHeart,
            BodyPartDefOf.Heart,
            patient => patient.AddHediff(Extra.HediffDefOf.HeartArteryBlockage, BodyPartDefOf.Heart));

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BodyPartModded,
            Description = "[Doctor] installed a bionic heart on [PAWN] to treat an artery blockage (minor).",
            Concerns = [doctor],
        });
    }

    public void TestFail(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(
            scenario,
            Extra.RecipeDefOf.InstallBionicHeart,
            BodyPartDefOf.Heart,
            surgeryOutcome: SurgeryOutcomes.RidiculousFailure);

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BotchedSurgery,
            Description = "[Doctor] catastrophically botched the installation of a bionic heart on [PAWN], causing [NewInjuries].",
            Concerns = [doctor],
        });
    }
}
