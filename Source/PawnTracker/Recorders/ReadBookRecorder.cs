using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class ReadBookRecorder : HistoryTaleRecorder<ReadBookEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<ReadBookEvent>(CreateRecord);
    }

    public override void CreateRecord(ReadBookEvent e)
    {
        var (pawn, book) = e;
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

    public void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(15).Colonist().Execute();
        var book = BookUtility.MakeBook(ArtGenerationContext.Colony);

        foreach (var pawn in pawns)
        {
            TaleRecorder.RecordTale(TaleDefOf.ReadBook, pawn, book);
            TaleRecorder.RecordTale(TaleDefOf.ReadBook, pawn, book);

            scenario.Thing(null).AnyBook().CreateSingle();
            Expect.That(pawn).ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.ReadBook,
                Concerns = [book],
            });
            Expect.That(pawn.HistoryRecords.Where(r => r.def == HistoryRecordDefOf.ReadBook).ToList().Count).Equal(1);
        }
    }

    [SkipTest]
    public void TestSkipValidation(TestScenario scenario)
    {
        SkipDateCheck = true;
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
        SkipDateCheck = false;
        SkipOverlapCheck = false;
    }
}
