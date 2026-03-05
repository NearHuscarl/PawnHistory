using LudeonTK;
using PawnHistory.Source.PawnTracker;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace PawnHistory.Source;

/// <summary>
/// https://github.com/pardeike/Rimworld-Doorstop
/// </summary>
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method)]
public class ReloadableAttribute : Attribute { }

class NearDebugActionAttribute : DebugActionAttribute
{
    public NearDebugActionAttribute() : base(
            category: "Pawn History",
            name: null,
            requiresRoyalty: false,
            requiresIdeology: false,
            requiresBiotech: false,
            requiresAnomaly: false,
            requiresOdyssey: false,
            displayPriority: 0,
            hideInSubMenu: false
        )
    {

    }
}

class NearDebugOutputAttribute : DebugOutputAttribute
{
    public NearDebugOutputAttribute() : base(category: "Pawn History", true) { }
}

public static class DebugTools
{
    // Hot reload feature does not work with generic type
    // https://github.com/pardeike/Rimworld-Doorstop/issues/5
    public static object First(object ie)
    {
        return ((IEnumerable<Pawn>)ie).First();
    }
    public static Pawn[] AllPawns()
    {
        return Find.CurrentMap.mapPawns.AllPawns.ToArray();
    }
    public static Pawn[] AllCorpses()
    {
        return Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).Cast<Corpse>().Select(c => c.InnerPawn).ToArray();
    }

    [NearDebugOutput]
    public static void PawnHistoryRecords()
    {
        var options = new List<DebugMenuOption>();
        var alivePawns = Find.CurrentMap.mapPawns.AllPawns;
        var deadPawns = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).Cast<Corpse>().Select(c => c.InnerPawn);
        var pawns = alivePawns.Concat(deadPawns).Where(x => x.HasComp<CompHistory>()).OrderByDescending(x => CompHistoryManager.GetComp(x).records.Count);

        foreach (var pawn in pawns)
        {
            var compHistory = CompHistoryManager.GetComp(pawn);
            var label = $"{pawn.Name} ({compHistory.records.Count})";

            options.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, () =>
            {
                DebugTables.MakeTablesDialog(compHistory.records,
                    new TableDataGetter<HistoryRecord>("Date", r => r.date),
                    new TableDataGetter<HistoryRecord>("DateReadable", r => compHistory.GetShortDate(r)),
                    new TableDataGetter<HistoryRecord>("defName", r => r.eventDef.defName),
                    new TableDataGetter<HistoryRecord>("label", r => r.eventDef.label),
                    new TableDataGetter<HistoryRecord>("description", r => r.GetDescription())
                 );
            }));
        }
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }
}
