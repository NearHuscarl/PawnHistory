using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record ExhaustedEvent(Pawn Pawn) : GameEventBase;

file class ExhaustedDispatcher : TaleDispatcher
{
    public override TaleDef TaleDef => TaleDefOf.Exhausted;

    public override void Dispatch(TaleRecordedEvent e)
    {
        GameEventBus.Publish(new ExhaustedEvent(e.Pawn));
    }
}
