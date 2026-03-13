using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public abstract class RecorderBase
{
    protected bool ShouldRecord(Pawn pawn) => RecorderManager.ShouldRecord(pawn);

    public abstract void Register();

    protected void AddRecord(HistoryRecord record) => CompHistoryManager.GetComp(record.pawn).records.Add(record);
}
