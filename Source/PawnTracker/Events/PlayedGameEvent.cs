using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record PlayedGameEvent(Pawn Pawn, ThingDef ObjectDef) : GameEventBase;

file class PlayedGameDispatcher : TaleDispatcher
{
    public override TaleDef TaleDef => Extra.TaleDefOf.PlayedGame;

    public override void Dispatch(TaleRecordedEvent e)
    {
        if (e.Params.ElementAtOrDefault(0) is ThingDef objectDef)
            GameEventBus.Publish(new PlayedGameEvent(e.Pawn, objectDef));
    }
}
