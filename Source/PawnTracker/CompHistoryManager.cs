using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Ui;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker;

[HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
internal class Game_LoadGame_Patch
{
    private static void Prefix() => CompHistoryManager.ClearAll();
}

internal static class CompHistoryManager
{
    public static readonly Dictionary<int, CompHistory> CompCache = [];
    public static readonly HashSet<int> TrackingDefHash = [];
    private static readonly Dictionary<int, List<PendingPriorityRecordWrite>> PendingPriorityRecords = [];
    private static int nextPrioritySequence;

    public static CompHistory GetComp(Pawn pawn)
    {
        if (pawn == null)
            return null;
        if (CompCache.TryGetValue(pawn.thingIDNumber, out var compCached))
            return compCached;
        if (!TrackingDefHash.Contains(pawn.def.shortHash))
            return null;
        var comp = pawn.GetComp<CompHistory>();
        if (comp != null)
            CompCache.Add(pawn.thingIDNumber, comp);
        return comp;
    }
    
    public static HistoryRecord WriteRecord(HistoryRecordWriteRequest request)
    {
        return WriteRecord(request.Def, request.Pawn, () => request);
    }

    public static HistoryRecord WriteRecord(HistoryRecordDef def, Pawn pawn, Func<HistoryRecordWriteRequest> resolveRequest)
    {
        return WriteRecord<object>(def, pawn, null, _ => resolveRequest());
    }

    public static HistoryRecord WriteRecord<T>(HistoryRecordDef def, Pawn pawn, T input, Func<List<T>, HistoryRecordWriteRequest> resolveRequest) where T : class
    {
        if (!HistoryRecordPriority.TryGetPriority(def, out var priority))
            return WriteRecordNow(resolveRequest(input == null ? [] : [input]));

        var tick = GenTicks.TicksAbs;
        if (!PendingPriorityRecords.TryGetValue(tick, out var pending))
        {
            pending = [];
            PendingPriorityRecords.Add(tick, pending);
            TickDelayManager.Delay(0, () => FlushPriorityRecords(tick));
        }

        pending.Add(new PendingPriorityRecordWrite(def, pawn, priority, nextPrioritySequence++, input, inputs => resolveRequest(inputs.Cast<T>().ToList())));
        return null;
    }

    private static void FlushPriorityRecords(int tick)
    {
        if (!PendingPriorityRecords.Remove(tick, out var pending) || pending.Count == 0)
            return;

        foreach (var pawnGroup in pending.GroupBy(p => p.Pawn))
        {
            var orderedEntries = pawnGroup.OrderBy(e => e.Priority).ThenBy(e => e.Sequence).ToList();
            var handledAggregateDefs = new HashSet<HistoryRecordDef>();

            foreach (var entry in orderedEntries)
            {
                if (entry.Input == null)
                {
                    WriteRecordNow(entry.ResolveRequest([]));
                    continue;
                }

                if (!handledAggregateDefs.Add(entry.Def))
                    continue;

                var inputs = orderedEntries
                    .Where(e => e.Input != null && e.Def == entry.Def)
                    .Select(e => e.Input)
                    .ToList();
                WriteRecordNow(entry.ResolveRequest(inputs));
            }
        }
    }

    public static void ClearAll()
    {
        CompCache.Clear();
        PendingPriorityRecords.Clear();
        nextPrioritySequence = 0;
    }

    private static HistoryRecord WriteRecordNow(HistoryRecordWriteRequest request)
    {
        var comp = GetComp(request.Pawn);
        var record = new HistoryRecord(
            request.Def,
            request.Pawn,
            request.Desc,
            request.Concerns,
            request.Location,
            request.TileId,
            request.Quest);

        comp.records.Add(record);
        return record;
    }

    public static void AttachHistoryComp()
    {
        var defsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;

        foreach (var thingDef in defsListForReading)
        {
            if (!RecorderManager.ShouldRecord(thingDef) || thingDef.IsCorpse)
                continue;
            
            thingDef.comps.Add(new CompProperties_History());
            TrackingDefHash.Add(thingDef.shortHash);
            var type = typeof(ITab_Pawn_History);
            var sharedInstance = InspectTabManager.GetSharedInstance(type);

            thingDef.inspectorTabs?.AddDistinct(type);
            thingDef.inspectorTabsResolved?.AddDistinct(sharedInstance);

            if (thingDef.race?.corpseDef != null)
            {
                thingDef.race.corpseDef.inspectorTabs?.AddDistinct(type);
                thingDef.race.corpseDef.inspectorTabsResolved?.AddDistinct(sharedInstance);
            }
            else
                Log.Warning("[ModName] thingDef.race?.corpseDef == null for thingDef = " + thingDef.defName);
        }
    }
}

internal readonly record struct PendingPriorityRecordWrite(
    HistoryRecordDef Def,
    Pawn Pawn,
    int Priority,
    int Sequence,
    object Input,
    Func<List<object>, HistoryRecordWriteRequest> ResolveRequest);

internal record struct HistoryRecordWriteRequest(
    HistoryRecordDef Def,
    Pawn Pawn,
    string Desc,
    IEnumerable<Thing> Concerns = null,
    RecordLocation Location = null,
    int? TileId = null,
    Quest Quest = null);
