using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class ReadBookRecorder : HistoryTaleRecorder<ReadBookRecorder.Input>
{
    public record Input(Pawn pawn, Book book);

    public override void Register()
    {
        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale.def != TaleDefOf.ReadBook)
                return;

            CreateRecord(new Input(e.Pawn, e.Params[0] as Book));
        });
    }

    public override void CreateRecord(Input input)
    {
        var (pawn, book) = input;
        var recordDef = HistoryRecordDefOf.ReadBook;
        var bookTitle = book.Title.Colorize(ColoredText.SubtleGrayColor);
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .AddRule("Book", bookTitle)
            .Resolve();

        if (!ShouldRecordTale(pawn, recordDef, desc))
            return;

        AddRecord(recordDef, pawn, desc, [book]);
    }

    [SkipTest]
    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(15).Colonist().Execute();

        foreach (var pawn in pawns)
        {
            var book = BookUtility.MakeBook(ArtGenerationContext.Colony);
            GenPlace.TryPlaceThing(book, pawn.Position, pawn.Map, ThingPlaceMode.Near);

            TaleRecorder.RecordTale(TaleDefOf.ReadBook, pawn, book);
            TaleRecorder.RecordTale(TaleDefOf.ReadBook, pawn, book);
        }
    }

    [SkipTest]
    public void TestSkipValidation(TestScenario scenario)
    {
        skipDateCheck = true;
        var pawns = scenario.Pawn(15).Colonist().Execute();

        foreach (var pawn in pawns)
        {
            var book = BookUtility.MakeBook(ArtGenerationContext.Colony);
            GenPlace.TryPlaceThing(book, pawn.Position, pawn.Map, ThingPlaceMode.Near);

            TaleRecorder.RecordTale(TaleDefOf.ReadBook, pawn, book);
            TaleRecorder.RecordTale(TaleDefOf.ReadBook, pawn, book);
            TaleRecorder.RecordTale(TaleDefOf.ReadBook, pawn, book);
            TaleRecorder.RecordTale(TaleDefOf.ReadBook, pawn, book);
            TaleRecorder.RecordTale(TaleDefOf.ReadBook, pawn, book);
            TaleRecorder.RecordTale(TaleDefOf.ReadBook, pawn, book);
        }
        skipDateCheck = false;
        skipOverlapCheck = false;
    }
}
