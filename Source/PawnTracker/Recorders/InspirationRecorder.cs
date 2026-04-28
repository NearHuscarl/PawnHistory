using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class InspirationRecorder : RecorderBase<InspirationStartedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<InspirationStartedEvent>(CreateRecord);
    }

    public override void CreateRecord(InspirationStartedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        // TODO: support other causes
        if (e.Data.Cause != InspirationCause.HighMood && e.Data.Cause != InspirationCause.Trait)
            return;

        var recordDef = HistoryRecordDefOf.Inspiration;
        var desc = recordDef.Description(e.Pawn)
            .AddRule("Inspiration", e.Inspiration.label.Colorize(ColoredText.ColonistCountColor))
            .AddRule("Trait", e.Data.AffectedByTrait?.CurrentData.label.Colorize(NeedsCardUtility.MoodColorNegative))
            .AddConstant("reason", e.Data.Cause)
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc, [e.Data.Initiator]);
    }

    public void TestHighMood(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .FullHeal()
            .ResetSkillLevel(SkillDefOf.Social, 20)
            .CreateSingle();

        pawn.mindState.inspirationHandler.TryStartInspiration(InspirationDefOf.Inspired_Recruitment, "LetterInspirationBeginThanksToHighMoodPart".Translate());

        Expect.That(pawn).ToHaveHistoryRecord("Thanks to high mood, [PAWN] gained an inspiration: Inspired recruitment.", HistoryRecordDefOf.Inspiration);
    }

    public void TestTrait(TestScenario scenario)
    {
        var trait = (Trait)null;
        var pawn = scenario.Pawn()
            .Colonist()
            .FullHeal()
            .ResetSkillLevel(SkillDefOf.Artistic, 20)
            .GiveTrait(DefLookup.Trait.TorturedArtist, traitCreated: t => trait = t)
            .CreateSingle();

        trait.Notify_MentalStateEndedOn(pawn);

        Expect.That(pawn).ToHaveHistoryRecord("After a mental break, [PAWN], who has the tortured artist trait, gained an inspiration: Inspired creativity.", HistoryRecordDefOf.Inspiration);
    }
}
