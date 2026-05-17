using System;
using PawnHistory.Source.Helper;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.HistoryBackfill;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public interface IRecordComp
{
    Type RecorderType { get; }
}

public abstract class RecordComp<TRecorder> : IRecordComp where TRecorder : RecorderBase
{
    public Type RecorderType => typeof(TRecorder);
}

public interface IRecord<in T>
{
    void CreateRecord(T input);
}

public abstract class RecorderBase
{
    internal RecorderBase() { } // only allow RecorderBase<T> to create instance

    protected bool ShouldRecord(Pawn pawn) => RecorderManager.ShouldRecord(pawn);
    public abstract void Register();
    internal virtual IEnumerable<HistoryBackfillDefinition> GetBackfillDefinitions() => [];
    internal virtual HistoryRecordWriteRequest FinalizePriorityWriteRequest(HistoryRecordWriteRequest request) => request;

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
    
    public readonly List<IRecordComp> Comps = [];
    public void AddComp(IRecordComp comp)
    {
        Comps.Add(comp);
    }
    
    protected virtual float DaysToRecordAgain => -1f;
    protected bool IsTooSoonToRecordAgain(Pawn pawn, HistoryRecordDef recordDef, float? daysToRecordAgainOverride = null)
    {
        var lastRecord = GeRecordsOfType(pawn, recordDef).FirstOrDefault();
        if (lastRecord == null)
            return false;

        return GenTicks.TicksAbs - lastRecord.date < GenDate.DaysToTicks(daysToRecordAgainOverride ?? DaysToRecordAgain);
    }

    protected HistoryRecord AddRecord(
        HistoryRecordDef def,
        Pawn pawn,
        string desc,
        IEnumerable<Thing> concerns = null,
        RecordLocation location = null,
        int? tileId = null,
        Quest quest = null)
    {
        return CompHistoryManager.WriteRecord(new HistoryRecordWriteRequest(def, pawn, desc, concerns, location, tileId, quest), this);
    }
}

public abstract class RecorderBase<TInput> : RecorderBase, IRecord<TInput>
{
    public abstract override void Register();
    public abstract void CreateRecord(TInput input);
}
