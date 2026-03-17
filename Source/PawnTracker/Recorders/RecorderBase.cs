using PawnHistory.Source.PawnTracker.Test;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public abstract class RecorderBase
{
    protected bool ShouldRecord(Pawn pawn) => RecorderManager.ShouldRecord(pawn);

    public abstract void Register();

    protected void AddRecord(HistoryRecordDef def, Pawn pawn, TaggedString resolvedDesc, IEnumerable<Thing> concerns = null)
    {
        CompHistoryManager.GetComp(pawn).records.Add(new HistoryRecord(def, pawn, resolvedDesc, concerns));
    }

    public virtual void Test(TestScenario testScenario)
    {
        Log.Message($"Test in {GetType().Name} is not implemented yet.");
    }
}
