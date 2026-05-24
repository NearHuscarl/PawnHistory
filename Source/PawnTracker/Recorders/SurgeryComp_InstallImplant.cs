using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using PawnHistory.Source.PawnTracker.Test.Mocks;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class SurgeryComp_InstallImplant : SurgeryComp
{
    public override bool Match(BuildInput input) => input.Event is SurgeryInstallImplantEvent;

    public override HistoryRecordDef RecordDef(BuildInput input) => HistoryRecordDefOf.BodyPartImplanted;

    public override HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var e = (SurgeryInstallImplantEvent)input.Event;
        return builder.AddRule("ImplantHediff", e.HediffToAdd, addSubsymbols: true);
    }

    public override HistoryDescriptionBuilder BuildBotchedGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        var e = (SurgeryInstallImplantEvent)input.Event;
        return builder.AddRule("ImplantHediff", e.HediffToAdd, addSubsymbols: true);
    }

    public void Test(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(scenario, Extra.RecipeDefOf.InstallJoywire, Extra.BodyPartDefOf.Brain);

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BodyPartImplanted,
            Description = "[Doctor] installed a joywire in [PAWN]'s brain.",
            Concerns = [doctor],
        });
    }

    public void TestFail(TestScenario scenario)
    {
        var (patient, doctor) = SurgeryRecorder.DoSurgery(scenario, Extra.RecipeDefOf.InstallJoywire, Extra.BodyPartDefOf.Brain, surgeryOutcome: SurgeryOutcomes.CatastrophicFailure);

        Expect.That(patient).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BotchedSurgery,
            Description = "[Doctor] catastrophically botched the implantation of a joywire on [PAWN], causing [NewInjuries].",
            Concerns = [doctor],
        });
    }
}
