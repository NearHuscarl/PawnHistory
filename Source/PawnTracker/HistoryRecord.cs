using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public class HistoryRecord : IExposable
{
    /// <summary>
    /// Empty constructor is required so Scribe can instantiate it
    /// </summary>
    public HistoryRecord() => date = GenTicks.TicksAbs;
    public HistoryRecord(HistoryRecordDef def, Pawn pawn, TaggedString desc, IEnumerable<Thing> concerns = null) : this()
    {
        this.def = def;
        this.pawn = pawn ?? throw new ArgumentNullException(nameof(pawn));
        this.description = desc.Resolve();
        this.concerns = new List<Thing> { pawn }.Concat(concerns ?? []).Where(p => p != null).Distinct().ToList();
        
        CurrentPawnToJumpTo = 0;
    }

    public HistoryRecordDef def;
    public int date;
    public string description;
    public Pawn pawn;
    public List<Thing> concerns;
    public int CurrentPawnToJumpTo { get; private set; }

    public Texture2D GetIcon()
    {
        return ContentFinder<Texture2D>.Get(def.icon);
    }

    public Thing GetThingToJumpTo()
    {
        CurrentPawnToJumpTo = (CurrentPawnToJumpTo + 1) % concerns.Count;

        var selectedThing = Find.Selector.SingleSelectedThing;

        if (selectedThing == concerns[CurrentPawnToJumpTo].GetJumpTarget())
            CurrentPawnToJumpTo = (CurrentPawnToJumpTo + 1) % concerns.Count;

        return concerns[CurrentPawnToJumpTo].GetJumpTarget();
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref def, "def");
        Scribe_Values.Look(ref date, "date");
        Scribe_Values.Look(ref description, "d");
        Scribe_References.Look(ref pawn, "pawn", saveDestroyedThings: true);
        Scribe_Collections.Look(ref concerns, "concerns", saveDestroyedThings: true, LookMode.Reference);
    }
}