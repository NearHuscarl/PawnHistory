using System;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Recorders;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public class CompHistory : ThingComp
{
    public List<HistoryRecord> records;
    private Pawn Pawn => parent as Pawn ?? throw new ArgumentNullException(nameof(Pawn));
    public HistoryRecord PawnGeneratedRecord;
    public SkillLevelChangedState SkillLevelChangedState;
    
    public CompHistory() => EnsureInitialized();

    public void ClearAll()
    {
        records = [];
        PawnGeneratedRecord = null;
        SkillLevelChangedState = new SkillLevelChangedState();
    }

    public bool RemoveRecord(HistoryRecord record) => records.Remove(record);

    private void EnsureInitialized()
    {
        records ??= [];
        SkillLevelChangedState ??= new SkillLevelChangedState();
        
        foreach (var record in records.ToList())
        {
            if (record.def == HistoryRecordDefOf.PawnGenerated)
                PawnGeneratedRecord = record;
            
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
        Scribe_Deep.Look(ref SkillLevelChangedState, "skillLevelChangedState");

        if (Scribe.mode != LoadSaveMode.PostLoadInit)
            return;

        EnsureInitialized();
    }
}
