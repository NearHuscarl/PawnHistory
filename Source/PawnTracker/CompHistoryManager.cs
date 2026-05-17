using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Recorders;
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
    
    public static HistoryRecord WriteRecord(HistoryRecordWriteRequest request, RecorderBase recorder = null)
    {
        if (!HistoryRecordPriority.TryGetPriority(request.Def, out var priority))
            return WriteRecordNow(request);

        var tick = GenTicks.TicksAbs;
        if (!PendingPriorityRecords.TryGetValue(tick, out var pending))
        {
            pending = [];
            PendingPriorityRecords.Add(tick, pending);
            TickDelayManager.Delay(0, () => FlushPriorityRecords(tick));
        }

        pending.Add(new PendingPriorityRecordWrite(request, recorder, priority, nextPrioritySequence++));
        return null;

    }

    private static void FlushPriorityRecords(int tick)
    {
        if (!PendingPriorityRecords.Remove(tick, out var pending) || pending.Count == 0)
            return;

        foreach (var pawnGroup in pending.GroupBy(p => p.Request.Pawn))
        {
            foreach (var entry in pawnGroup.OrderBy(e => e.Priority).ThenBy(e => e.Sequence))
                WriteRecordNow(entry.Recorder.FinalizePriorityWriteRequest(entry.Request));
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

internal readonly record struct PendingPriorityRecordWrite(HistoryRecordWriteRequest Request, RecorderBase Recorder, int Priority, int Sequence);

internal record struct HistoryRecordWriteRequest(
    HistoryRecordDef Def,
    Pawn Pawn,
    string Desc,
    IEnumerable<Thing> Concerns = null,
    RecordLocation Location = null,
    int? TileId = null,
    Quest Quest = null);
