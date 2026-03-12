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
    public HistoryRecord(PawnEventDef eventDef, Pawn pawn, TaggedString resolvedDesc, List<Pawn> relatedPawns = null) : this()
    {
        this.eventDef = eventDef;
        this.pawn = pawn ?? throw new ArgumentNullException(nameof(pawn));
        this.resolvedDesc = resolvedDesc;
        CurrentPawnToJumpTo = 0;
        concerns = new List<Pawn> { pawn }.Concat(relatedPawns ?? []).Where(p => p != null).Concat(pawn).Distinct().ToList();
    }

    public PawnEventDef eventDef;
    public int date;
    public TaggedString resolvedDesc;
    public Pawn pawn;
    public List<Pawn> concerns;
    public int CurrentPawnToJumpTo { get; private set; }

    public Texture2D GetIcon()
    {
        return ContentFinder<Texture2D>.Get(eventDef.icon);
    }

    public TaggedString GetDescription()
    {
        return resolvedDesc;
    }

    public Thing GetThingToJumpTo()
    {
        CurrentPawnToJumpTo = (CurrentPawnToJumpTo + 1) % concerns.Count;

        var selectedThing = Find.Selector.SingleSelectedThing;

        if (selectedThing == concerns[CurrentPawnToJumpTo].SpawnedThing())
            CurrentPawnToJumpTo = (CurrentPawnToJumpTo + 1) % concerns.Count;

        return concerns[CurrentPawnToJumpTo].SpawnedThing();
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref eventDef, "eventDef");
        Scribe_Values.Look(ref date, "date");
        Scribe_Values.Look(ref resolvedDesc, "d");
        Scribe_References.Look(ref pawn, "pawn", saveDestroyedThings: true);
        Scribe_Collections.Look(ref concerns, "concerns", saveDestroyedThings: true, LookMode.Reference);
    }
}