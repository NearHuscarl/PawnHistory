using PawnHistory.Source.DebugTools;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public class RecordLocation : IExposable
{
    public IntVec3 position;
    public Map map;

    public static RecordLocation Of(Thing thing) => new()
    {
        position = thing.Position,
        map = thing.Map
    };

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
    public HistoryRecord(
        HistoryRecordDef def,
        Pawn pawn,
        string desc,
        IEnumerable<Thing> concerns = null,
        RecordLocation location = null,
        int? tileId = null,
        Quest quest = null) : this()
    {
        this.def = def;
        this.pawn = pawn ?? throw new ArgumentNullException(nameof(pawn));
        this.description = desc;
        this.concerns = (concerns ?? []).Where(p => p != null && p != pawn).Distinct().ToList();
        this.date = GenTicks.TicksAbs;
        this.tileId = tileId ?? location?.map?.Tile.tileId ?? pawn.GetTileId();
        
        if (quest is { hidden: false })
            this.quest = quest;
        
        if (this.tileId == -1)
            Log.Message($"[PawnHistory] record for {pawn} is created but cannot find tileId this early during world generation.\n\n{DebugUtility.Format(this)}");
        else if (pawn.IsWorldPawn() && pawn.MapHeld == null && this.tileId == Find.AnyPlayerHomeMap?.Tile.tileId)
            Log.Message($"[PawnHistory] record for {pawn} is created but cannot find tileId, falling back to PlayerHomeMap's tile..\n\n{DebugUtility.Format(this)}");

        this.location = location;

        CurrentPawnToJumpTo = 0;
    }

    public HistoryRecordDef def;
    public int date;
    public Pawn pawn;
    public string description;
    public List<Thing> concerns;
    public int tileId;
    public RecordLocation location;
    public Quest quest;
    public bool pinned;
    public int CurrentPawnToJumpTo { get; private set; }

    private static readonly Dictionary<string, Texture2D> CachedIconTextures = [];
    public Texture2D Icon
    {
        get
        {
            if (!CachedIconTextures.TryGetValue(def.icon, out var icon))
            {
                icon = ContentFinder<Texture2D>.Get(def.icon);
                CachedIconTextures.Add(def.icon, icon);
            }
            return icon;
        }
    }

    public IEnumerable<Thing> ConcernedThings
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

            foreach (var target in ConcernedThings)
                yield return target;
        }
    }

    public Thing GetThingToJumpTo()
    {
        var targets = ConcernedThings.ToList();

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
        Scribe_References.Look(ref quest, "quest");
        Scribe_Values.Look(ref pinned, "pinned");
    }

    public override string ToString() => $"{def.defName}_{date}";
}

public static class HistoryRecordExtensions
{
    public static string GetShortDate(this HistoryRecord record)
    {
        return DateHelper.GetShortDate(record.date, record.tileId);
    }

    public static string GetTipDate(this HistoryRecord record)
    {
        var position = Find.WorldGrid.LongLatOf(WorldGridUtility.Tile(record.tileId));
        return GenDate.DateFullStringWithHourAt(record.date, position);
    }
}
