using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public class CompHistory : ThingComp
{
    public List<HistoryRecord> records;
    private Pawn Pawn => parent as Pawn ?? throw new ArgumentNullException(nameof(Pawn));
    public HistoryRecord PawnGeneratedRecord;

    public CompHistory() => EnsureInitialized();

    private Dictionary<string, float> pawnRecordSnapshot;

    internal void UpdateSnapshot(RecordDef[] recordDefs)
    {
        foreach (var def in recordDefs)
        {
            if (pawnRecordSnapshot.ContainsKey(def.defName))
                pawnRecordSnapshot[def.defName] = Pawn.records.GetValue(def);
            else
                pawnRecordSnapshot.Add(def.defName, Pawn.records.GetValue(def));
        }
    }

    public record RecordDelta(RecordDef Def, float Delta);

    public RecordDelta DeltaFrom(RecordDef def)
    {
        var previous = pawnRecordSnapshot.TryGetValue(def.defName, 0);
        var now = Pawn.records.GetValue(def);
        var delta = now - previous;
        
        return new RecordDelta(def, delta);
    }

    private Dictionary<string, string> recordsShownInCurrentRound;

    /// <summary>
    /// Returns the highest-delta record not yet shown this round for <paramref name="skill"/>,
    /// cycling through all eligible records before allowing repeats.
    /// </summary>
    public RecordDelta DominantDelta(SkillDef skill, IReadOnlyCollection<RecordDef> recordDefs)
    {
        RecordDelta best = null;

        recordsShownInCurrentRound.TryAdd(skill.defName, "");

        var recordsShown = recordsShownInCurrentRound[skill.defName].Split('|', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        foreach (var def in recordDefs)
        {
            var delta = DeltaFrom(def);
            if (delta.Delta <= 0 || recordsShown.Contains(def.defName)) continue;

            if (best is null || delta.Delta > best.Delta)
                best = delta;
        }

        // Round exhausted - reset and try again
        if (best is null)
        {
            recordsShownInCurrentRound[skill.defName] = "";

            foreach (var def in recordDefs)
            {
                var delta = DeltaFrom(def);
                if (delta.Delta <= 0) continue;

                if (best is null || delta.Delta > best.Delta)
                    best = delta;
            }
        }

        if (best is not null)
            recordsShownInCurrentRound[skill.defName] += "|" + best.Def.defName;

        return best;
    }

    public void ClearAll()
    {
        records = [];
        pawnRecordSnapshot = [];
        recordsShownInCurrentRound = [];
    }

    private void EnsureInitialized()
    {
        records ??= [];
        pawnRecordSnapshot ??= [];
        recordsShownInCurrentRound ??= [];

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
        Scribe_Collections.Look(ref pawnRecordSnapshot, "pawnRecordSnapshot");
        Scribe_Collections.Look(ref recordsShownInCurrentRound, "recordsShownInCurrentRound");

        if (Scribe.mode != LoadSaveMode.PostLoadInit)
            return;

        EnsureInitialized();
    }
}
