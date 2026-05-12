using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record StripEvent(Pawn Pawn, Pawn StrippedPawn) : GameEventBase;

file class StripDispatcher() : TaleDispatcher
{
    public override TaleDef TaleDef => Extra.TaleDefOf.Stripped;

    public override void Dispatch(TaleRecordedEvent e)
    {
        if (e.Params.ElementAtOrDefault(0) is Pawn strippedPawn)
            GameEventBus.Publish(new StripEvent(e.Pawn, strippedPawn));
    }
}
