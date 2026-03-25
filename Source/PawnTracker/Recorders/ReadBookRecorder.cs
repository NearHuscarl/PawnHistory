using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class ReadBookRecorder : HistoryTaleRecorder
{
    public override void Register()
    {
        GameEventBus.Subscribe<TaleRecordedEvent>(e =>
        {
            if (e.Tale.def != TaleDefOf.ReadBook)
                return;

            HandleReadBookEvent(e);
        });
    }

    private void HandleReadBookEvent(TaleRecordedEvent e)
    {
        var recordDef = HistoryRecordDefOf.ReadBook;
        var book = e.Params[0] as Book;
        var bookTitle = book.Title.ApplyTag(TagType.Reward);
        var desc = recordDef.Description("readbook", e.Pawn)
            .IncludePawnGrammar()
            .AddRule("Book", bookTitle)
            .Resolve();

        if (!ShouldRecordTale(e.Pawn, recordDef, desc))
            return;

        AddRecord(recordDef, e.Pawn, desc, [book]);
    }

    public override void Test(TestScenario scenario)
    {
        var pawns = scenario.Pawn(15).Colonist().Create();

        foreach (var pawn in pawns)
        {
            var book = BookUtility.MakeBook(ArtGenerationContext.Colony);
            GenPlace.TryPlaceThing(book, pawn.Position, pawn.Map, ThingPlaceMode.Near);

            TaleRecorder.RecordTale(TaleDefOf.ReadBook, pawn, book);
            TaleRecorder.RecordTale(TaleDefOf.ReadBook, pawn, book);
        }
    }

    public void TestSkipValidation(TestScenario scenario)
    {
        skipDateCheck = true;
        var pawns = scenario.Pawn(15).Colonist().Create();

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
