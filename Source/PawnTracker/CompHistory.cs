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

    public readonly struct RecordDelta(RecordDef def, float delta)
    {
        public RecordDef Def { get; } = def;
        public float Delta { get; } = delta;
    }

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
    public RecordDelta? DominantDelta(SkillDef skill, IReadOnlyCollection<RecordDef> recordDefs)
    {
        RecordDelta? best = null;

        if (!recordsShownInCurrentRound.ContainsKey(skill.defName))
            recordsShownInCurrentRound.Add(skill.defName, "");

        var recordsShown = recordsShownInCurrentRound[skill.defName].Split('|', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        foreach (var def in recordDefs)
        {
            var delta = DeltaFrom(def);
            if (delta.Delta <= 0 || recordsShown.Contains(def.defName)) continue;

            if (best is null || delta.Delta > best.Value.Delta)
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

                if (best is null || delta.Delta > best.Value.Delta)
                    best = delta;
            }
        }

        if (best is not null)
            recordsShownInCurrentRound[skill.defName] += "|" + best.Value.Def.defName;

        return best;
    }

    private void EnsureInitialized()
    {
        records ??= [];
        pawnRecordSnapshot ??= [];
        recordsShownInCurrentRound ??= [];

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
        Scribe_Collections.Look(ref pawnRecordSnapshot, "pawnRecordSnapshot");
        Scribe_Collections.Look(ref recordsShownInCurrentRound, "recordsShownInCurrentRound");

        if (Scribe.mode != LoadSaveMode.PostLoadInit)
            return;

        EnsureInitialized();
    }
}
