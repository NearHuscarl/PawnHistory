using HarmonyLib;
using System.Collections.Generic;
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
        var comp = GetComp(request.Pawn);
        var record = new HistoryRecord(
            request.Def,
            request.Pawn,
            request.ResolvedDesc,
            request.Concerns,
            request.Location,
            request.TileId,
            request.Quest);

        comp.records.Add(record);
        return record;
    }

    public static void ClearAll() => CompCache.Clear();

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

internal readonly record struct HistoryRecordWriteRequest(
    HistoryRecordDef Def,
    Pawn Pawn,
    TaggedString ResolvedDesc,
    IEnumerable<Thing> Concerns = null,
    RecordLocation Location = null,
    int? TileId = null,
    Quest Quest = null);