using PawnHistory.Source.DebugTools;
using RimWorld;
using RimWorld.Planet;
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
    public HistoryRecord() {}
    public HistoryRecord(HistoryRecordDef def, Pawn pawn, TaggedString desc, IEnumerable<Thing> concerns = null) : this()
    {
        this.def = def;
        this.pawn = pawn ?? throw new ArgumentNullException(nameof(pawn));
        this.description = desc.Resolve();
        this.concerns = new List<Thing> { pawn }.Concat(concerns ?? []).Where(p => p != null).Distinct().ToList();
        this.date = GenTicks.TicksAbs;

        if (pawn.IsWorldPawn())
        {
            Log.Message($"{nameof(HistoryRecord)} is initialized but cannot locate WorldPawn location, falling back to PlayerHomeMap..\n\n{DebugUtility.Format(this)}");
            this.tileId = Find.AnyPlayerHomeMap.Tile.tileId;
        }
        else
            this.tileId = pawn.WorldLocation().tileId;

        CurrentPawnToJumpTo = 0;
    }

    public HistoryRecordDef def;
    public int date;
    public int tileId;
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

        if (selectedThing == concerns[CurrentPawnToJumpTo].GetSpawnedHolderOrSelf())
            CurrentPawnToJumpTo = (CurrentPawnToJumpTo + 1) % concerns.Count;

        return concerns[CurrentPawnToJumpTo].GetSpawnedHolderOrSelf();
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref def, "def");
        Scribe_Values.Look(ref date, "date");
        Scribe_Values.Look(ref tileId, "tileId");
        Scribe_Values.Look(ref description, "d");
        Scribe_References.Look(ref pawn, "pawn", saveDestroyedThings: true);
        Scribe_Collections.Look(ref concerns, "concerns", saveDestroyedThings: true, LookMode.Reference);
    }
}

public static class HistoryRecordExtensions
{
    public static PlanetTile Tile(int tileId)
    {
        if (tileId < 0)
            return Find.AnyPlayerHomeMap?.Tile ?? PlanetTile.Invalid;
        return Find.WorldGrid[tileId].tile;
    }

    public static string GetShortDate(this HistoryRecord record)
    {
        var position = Find.WorldGrid.LongLatOf(Tile(record.tileId));
        var hourInt = GenDate.HourInteger(record.date, position.x);
        var hour = $"{hourInt}h";

        if (Prefs.TwelveHourClockMode)
        {
            var ampm = hourInt >= 12 ? "PM" : "AM";
            hourInt %= 12;
            if (hourInt == 0) hourInt = 12;
            hour = $"{hourInt} {ampm}";
        }

        var day = GenDate.DayOfYear(record.date, position.x) + 1;
        var year = GenDate.Year(record.date, position.x);
        return $"Y{year} D{day} {hour}";
    }

    public static string GetTipDate(this HistoryRecord record)
    {
        var position = Find.WorldGrid.LongLatOf(Tile(record.tileId));
        return GenDate.DateFullStringWithHourAt(record.date, position);
    }
}
