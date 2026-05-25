using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record MinedValuableEvent(Pawn Pawn, ThingDef MineableThing) : GameEventBase;

file class MinedValuableDispatcher : TaleDispatcher
{
    public override TaleDef TaleDef => TaleDefOf.MinedValuable;

    public override void Dispatch(TaleRecordedEvent e)
    {
        if (e.Params.ElementAtOrDefault(0) is ThingDef mineableThing)
            GameEventBus.Publish(new MinedValuableEvent(e.Pawn, mineableThing));
    }
}
