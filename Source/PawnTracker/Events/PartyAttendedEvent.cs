using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record PartyAttendedEvent(Pawn Attender, Pawn Organizer) : GameEventBase;

file class PartyAttendedDispatcher : TaleDispatcher
{
    public override TaleDef TaleDef => TaleDefOf.AttendedParty;

    public override void Dispatch(TaleRecordedEvent e)
    {
        if (e.Params.ElementAtOrDefault(0) is Pawn organizer)
            GameEventBus.Publish(new PartyAttendedEvent(e.Pawn, organizer));
    }
}
