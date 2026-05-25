using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record WalkNakedEvent(Pawn Pawn) : GameEventBase;

file class WalkNakedDispatcher : TaleDispatcher
{
    public override TaleDef TaleDef => TaleDefOf.WalkedNaked;

    public override void Dispatch(TaleRecordedEvent e)
    {
        GameEventBus.Publish(new WalkNakedEvent(e.Pawn));
    }
}
