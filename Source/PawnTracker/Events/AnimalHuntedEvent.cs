using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record AnimalHuntedEvent(Pawn Hunter, Pawn Prey) : GameEventBase;

file class AnimalHuntedDispatcher() : TaleDispatcher
{
    public override TaleDef TaleDef => TaleDefOf.Hunted;

    public override void Dispatch(TaleRecordedEvent e)
    {
        if (e.Params.ElementAtOrDefault(0) is Pawn { RaceProps.Animal: true } prey)
            GameEventBus.Publish(new AnimalHuntedEvent(e.Pawn, prey));
    }
}
