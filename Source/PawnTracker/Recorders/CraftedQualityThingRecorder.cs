using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class CraftedQualityThingRecorder : RecorderBase<CraftedQualityThingEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<CraftedQualityThingEvent>(CreateRecord);
    }

    public override void CreateRecord(CraftedQualityThingEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var recordDef = HistoryRecordDefOf.CraftedQualityThing;

        if (e.Quality == QualityCategory.Masterwork && IsTooSoonToRecordAgain(e.Pawn, recordDef, 3f))
            return;
        
        var desc = recordDef.Description(e.Pawn)
            .IncludePawnGrammar()
            .AddRule("Quality", e.Quality.GetLabel().ToLower(), addSubsymbols: true)
            .AddRule("Crafted", e.CraftedThing.LabelNoParenthesis.Colorize(ColoredText.SubtleGrayColor), addSubsymbols: true)
            .Resolve();

        AddRecord(recordDef, e.Pawn, desc, [e.CraftedThing]);
    }

    public void TestMasterwork(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var craftedThing = scenario.Thing(ThingDefOf.Bed, ThingDefOf.Gold)
            .Quality(QualityCategory.Masterwork)
            .CreateSingle<Building_Bed>();
        
        craftedThing.GetComp<CompArt>().InitializeArt(ArtGenerationContext.Colony);
        QualityUtility.SendCraftNotification(craftedThing, pawn);

        Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.CraftedQualityThing,
            Description = "[PAWN] completed [His] work on a masterwork golden bed. [Image]. [DescSentence]",
            Concerns = [craftedThing],
        });
    }

    public void TestLegendary(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var craftedThing = scenario.Thing(ThingDefOf.Apparel_Parka, ThingDefOf.Leather_Plain)
            .Quality(QualityCategory.Legendary)
            .CreateSingle<Apparel>();
        
        QualityUtility.SendCraftNotification(craftedThing, pawn);

        Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.CraftedQualityThing,
            Description = "[PAWN] completed [His] work on a legendary plainleather parka. [Image]. [DescSentence]",
            Concerns = [craftedThing],
        });
    }
}
