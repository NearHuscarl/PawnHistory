using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public abstract class RecorderBase
{
    protected bool ShouldRecord(Pawn pawn) => RecorderManager.ShouldRecord(pawn);

    public abstract void Register();
}
