using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record ConcertHeldEvent(Pawn Organizer) : GameEventBase;

file class ConcertHeldDispatcher : TaleDispatcher
{
    public override TaleDef TaleDef => TaleDefOf.HeldConcert;

    public override void Dispatch(TaleRecordedEvent e)
    {
        GameEventBus.Publish(new ConcertHeldEvent(e.Pawn));
    }
}
