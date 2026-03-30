using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public class CompHistory : ThingComp
{
    public List<HistoryRecord> records;
    private Pawn Pawn => parent as Pawn ?? throw new ArgumentNullException(nameof(Pawn));

    public CompHistory() => EnsureInitialized();

    private void EnsureInitialized()
    {
        records ??= [];

        foreach (var record in records.ToList())
        {
            // remove corrupted records during development.
            if (record.pawn == null)
            {
                Log.Error($"HistoryRecord.pawn = null. {record.def}, {record.date}. WHY!?");
                records.Remove(record);
            }
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Collections.Look(ref records, "historyRecords", LookMode.Deep);
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
            return;

        EnsureInitialized();
    }
}
