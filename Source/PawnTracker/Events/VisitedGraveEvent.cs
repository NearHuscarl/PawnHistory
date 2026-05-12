using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record VisitedGraveEvent(Pawn Pawn, Pawn DeadPawn) : GameEventBase;

file class VisitedGraveDispatcher() : TaleDispatcher
{
    public override TaleDef TaleDef => Extra.TaleDefOf.VisitedGrave;

    public override void Dispatch(TaleRecordedEvent e)
    {
        if (e.Params.ElementAtOrDefault(0) is Pawn deadPawn)
            GameEventBus.Publish(new VisitedGraveEvent(e.Pawn, deadPawn));
    }
}
