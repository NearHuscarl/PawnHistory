using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record AttendedPartyEvent(Pawn Attender, Pawn Organizer) : GameEventBase;

file class AttendedPartyDispatcher : TaleDispatcher
{
    public override TaleDef TaleDef => TaleDefOf.AttendedParty;

    public override void Dispatch(TaleRecordedEvent e)
    {
        if (e.Params.ElementAtOrDefault(0) is Pawn organizer)
            GameEventBus.Publish(new AttendedPartyEvent(e.Pawn, organizer));
    }
}
