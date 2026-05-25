using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record OnFireEvent(Pawn Pawn) : GameEventBase;

file class OnFireDispatcher : TaleDispatcher
{
    public override TaleDef TaleDef => TaleDefOf.WasOnFire;

    public override void Dispatch(TaleRecordedEvent e)
    {
        GameEventBus.Publish(new OnFireEvent(e.Pawn));
    }
}
