using LudeonTK;
using PawnHistory.Source.PawnTracker;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;

namespace PawnHistory.Source;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method)]
public class ReloadableAttribute : Attribute { }

class NearDebugActionAttribute : DebugActionAttribute
{
    public NearDebugActionAttribute() : base(
            category: "PawnHistory",
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

public static class DebugTools
{
    // Hot reload feature does not work with generic type
    // https://github.com/pardeike/Rimworld-Doorstop/issues/5
    private static object First(object ie)
    {
        return ((IEnumerable<Pawn>)ie).First();
    }

    [Reloadable]
    [NearDebugAction]
    public static void LogRandomThings()
    {
        var p = Find.CurrentMap.mapPawns.AllHumanlike;

        foreach (var pawn in Find.CurrentMap.mapPawns.AllHumanlike)
        {
            var comp = CompHistoryManager.GetComp(pawn);

            Log.Message($"{pawn.Name}'s records:");
            foreach (var record in comp.records)
            {
                Log.Message($"{record.Date} {record.EventDef.defName} {record.EventDef.description}");
            }
            Log.Message("----------------\n");
        }

        Log.Message(First(p));

        //System.Diagnostics.Debugger.Break();
    }
}
