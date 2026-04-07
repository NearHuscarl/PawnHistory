using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Test;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public abstract class RecorderBase
{
    protected bool ShouldRecord(Pawn pawn) => RecorderManager.ShouldRecord(pawn);

    public abstract void Register();

    protected IEnumerable<HistoryRecord> GeRecordsOfType(Pawn pawn, HistoryRecordDef def, int limit = 100)
    {
        var records = CompHistoryManager.GetComp(pawn).records;

        for (var i = records.Count - 1; i >= 0; i--)
        {
            if (records.Count - 1 - i > limit)
                break;
            if (records[i].def == def)
                yield return records[i];
        }
    }

    protected virtual void AddRecord(HistoryRecordDef def, Pawn pawn, TaggedString resolvedDesc, IEnumerable<Thing> concerns = null, RecordLocation location = null)
    {
        pawn.GetHistoryRecords().Add(new HistoryRecord(def, pawn, resolvedDesc, concerns, location));
    }

    public virtual void Test(TestScenario testScenario)
    {
        Log.Message($"Test in {GetType().Name} is not implemented yet.");
    }
}
