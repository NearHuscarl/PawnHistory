using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record ReadBookEvent(Pawn Pawn, Book Book) : GameEventBase;

file class ReadBookDispatcher : TaleDispatcher
{
    public override TaleDef TaleDef => TaleDefOf.ReadBook;

    public override void Dispatch(TaleRecordedEvent e)
    {
        if (e.Params.ElementAtOrDefault(0) is Book book)
            GameEventBus.Publish(new ReadBookEvent(e.Pawn, book));
    }
}
