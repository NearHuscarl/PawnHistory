using PawnHistory.Source.Helper;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public interface IRecord<in T>
{
    void CreateRecord(T input);
}

public abstract class RecorderBase
{
    protected virtual float DaysToRecordAgain => -1f;

    internal RecorderBase() { } // only allow RecorderBase<T> to create instance

    protected bool ShouldRecord(Pawn pawn) => RecorderManager.ShouldRecord(pawn);
    public abstract void Register();

    protected IEnumerable<HistoryRecord> GeRecordsOfType(Pawn pawn, HistoryRecordDef def, int limit = 100)
    {
        var records = pawn.HistoryRecords;

        for (var i = records.Count - 1; i >= 0; i--)
        {
            if (records.Count - 1 - i > limit)
                break;
            if (records[i].def == def)
                yield return records[i];
        }
    }

    protected bool IsTooSoonToRecordAgain(Pawn pawn, HistoryRecordDef recordDef, float? daysToRecordAgainOverride = null)
    {
        var lastRecord = GeRecordsOfType(pawn, recordDef).FirstOrDefault();
        if (lastRecord == null)
            return false;

        return GenTicks.TicksAbs - lastRecord.date < GenDate.DaysToTicks(daysToRecordAgainOverride ?? DaysToRecordAgain);
    }

    protected virtual void AddRecord(HistoryRecordDef def, Pawn pawn, TaggedString resolvedDesc, IEnumerable<Thing> concerns = null, RecordLocation location = null, int? tileId = null)
    {
        pawn.HistoryRecords.Add(new HistoryRecord(def, pawn, resolvedDesc, concerns, location, tileId));
    }
}

public abstract class RecorderBase<TInput> : RecorderBase, IRecord<TInput>
{
    public abstract override void Register();
    public abstract void CreateRecord(TInput input);
}
