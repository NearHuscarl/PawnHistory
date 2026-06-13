using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record ConcertAttendedEvent(Pawn Attender, Pawn Organizer) : GameEventBase;

file class ConcertAttendedDispatcher : TaleDispatcher
{
    public override TaleDef TaleDef => TaleDefOf.AttendedConcert;

    public override void Dispatch(TaleRecordedEvent e)
    {
        if (e.Params.ElementAtOrDefault(0) is Pawn organizer)
            GameEventBus.Publish(new ConcertAttendedEvent(e.Pawn, organizer));
    }
}
