using PawnHistory.Source.Helper;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public interface IRecord<in T>
{
    void CreateRecord(T input);
}

public abstract class RecorderBase
{
    internal RecorderBase() { } // only allow RecorderBase<T> to create instance

    protected bool ShouldRecord(Pawn pawn) => RecorderManager.ShouldRecord(pawn);
    public abstract void Register();

    protected IEnumerable<HistoryRecord> GeRecordsOfType(Pawn pawn, HistoryRecordDef def, int limit = 100)
    {
        var records = pawn.GetHistoryRecords();

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
}

public abstract class RecorderBase<TInput> : RecorderBase, IRecord<TInput>
{
    public override abstract void Register();
    public abstract void CreateRecord(TInput input);
}
