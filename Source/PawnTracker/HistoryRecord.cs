using PawnHistory.Source.DebugTools;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public class RecordLocation : IExposable
{
    public IntVec3 position;
    public Map map;

    public void ExposeData()
    {
        Scribe_Values.Look(ref position, "position");
        Scribe_References.Look(ref map, "map");
    }
}

public class HistoryRecord : IExposable
{
    /// <summary>
    /// Empty constructor is required so Scribe can instantiate it
    /// </summary>
    public HistoryRecord() {}
    public HistoryRecord(HistoryRecordDef def, Pawn pawn, TaggedString desc, IEnumerable<Thing> concerns = null, bool storeLocation = false) : this()
    {
        this.def = def;
        this.pawn = pawn ?? throw new ArgumentNullException(nameof(pawn));
        this.description = desc.Resolve();
        this.concerns = (concerns ?? []).Where(p => p != null && p != pawn).Distinct().ToList();
        this.date = GenTicks.TicksAbs;

        if (pawn.IsWorldPawn())
        {
            Log.Message($"{nameof(HistoryRecord)} is initialized but cannot locate WorldPawn location, falling back to PlayerHomeMap..\n\n{DebugUtility.Format(this)}");
            this.tileId = Find.AnyPlayerHomeMap.Tile.tileId;
        }
        else
            this.tileId = pawn.WorldLocation().tileId;

        if (storeLocation && pawn.Spawned)
            this.location = new RecordLocation { position = pawn.Position, map = pawn.Map };

        CurrentPawnToJumpTo = 0;
    }

    public HistoryRecordDef def;
    public int date;
    public Pawn pawn;
    public string description;
    public List<Thing> concerns;
    public int tileId;
    public RecordLocation location;
    public int CurrentPawnToJumpTo { get; private set; }

    public Texture2D GetIcon()
    {
        return ContentFinder<Texture2D>.Get(def.icon);
    }

    public IEnumerable<Thing> AllTargets
    {
        get
        {
            yield return pawn;
            foreach (var concern in concerns) yield return concern;
        }
    }

    public IEnumerable<GlobalTargetInfo> GlobalTargets
    {
        get
        {
            if (location != null)
                yield return new GlobalTargetInfo(location.position, location.map, true);

            foreach (var target in AllTargets)
                yield return target;
        }
    }

    public Thing GetThingToJumpTo()
    {
        var targets = AllTargets.ToList();

        CurrentPawnToJumpTo = (CurrentPawnToJumpTo + 1) % targets.Count;

        var selectedThing = Find.Selector.SingleSelectedThing;

        if (selectedThing == targets[CurrentPawnToJumpTo].SpawnedParentOrMe)
            CurrentPawnToJumpTo = (CurrentPawnToJumpTo + 1) % targets.Count;

        return targets[CurrentPawnToJumpTo].SpawnedParentOrMe;
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref def, "def");
        Scribe_Values.Look(ref date, "date");
        Scribe_References.Look(ref pawn, "pawn", saveDestroyedThings: true);
        Scribe_Values.Look(ref description, "d");
        Scribe_Collections.Look(ref concerns, "concerns", saveDestroyedThings: true, LookMode.Reference);
        Scribe_Values.Look(ref tileId, "tileId");
        Scribe_Deep.Look(ref location, "location");
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
