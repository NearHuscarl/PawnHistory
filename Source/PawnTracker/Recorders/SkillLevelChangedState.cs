using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class SkillLevelChangedState : IExposable
{
    private Dictionary<string, float> pawnRecordSnapshot;

    public SkillLevelChangedState() => EnsureInitialized();

    private void EnsureInitialized()
    {
        pawnRecordSnapshot ??= [];
        recordsShownInCurrentRound ??= [];
    }

    internal void UpdateSnapshot(Pawn pawn, RecordDef[] recordDefs)
    {
        foreach (var def in recordDefs)
        {
            if (pawnRecordSnapshot.ContainsKey(def.defName))
                pawnRecordSnapshot[def.defName] = pawn.records.GetValue(def);
            else
                pawnRecordSnapshot.Add(def.defName, pawn.records.GetValue(def));
        }
    }

    public record RecordDelta(RecordDef Def, float Delta);

    public RecordDelta DeltaFrom(Pawn pawn, RecordDef def)
    {
        var previous = pawnRecordSnapshot.TryGetValue(def.defName, 0);
        var now = pawn.records.GetValue(def);
        var delta = now - previous;
        
        return new RecordDelta(def, delta);
    }

    private Dictionary<string, string> recordsShownInCurrentRound;

    /// <summary>
    /// Returns the highest-delta record not yet shown this round for <paramref name="skill"/>,
    /// cycling through all eligible records before allowing repeats.
    /// </summary>
    public RecordDelta DominantDelta(Pawn pawn, SkillDef skill, IReadOnlyCollection<RecordDef> recordDefs)
    {
        RecordDelta best = null;

        recordsShownInCurrentRound.TryAdd(skill.defName, "");

        var recordsShown = recordsShownInCurrentRound[skill.defName].Split('|', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        foreach (var def in recordDefs)
        {
            var delta = DeltaFrom(pawn, def);
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
                var delta = DeltaFrom(pawn, def);
                if (delta.Delta <= 0) continue;

                if (best is null || delta.Delta > best.Delta)
                    best = delta;
            }
        }

        if (best is not null)
            recordsShownInCurrentRound[skill.defName] += "|" + best.Def.defName;

        return best;
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref pawnRecordSnapshot, "pawnRecordSnapshot");
        Scribe_Collections.Look(ref recordsShownInCurrentRound, "recordsShownInCurrentRound");

        if (Scribe.mode != LoadSaveMode.PostLoadInit)
            return;

        EnsureInitialized();
    }
}