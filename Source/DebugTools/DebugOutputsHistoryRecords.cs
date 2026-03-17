using LudeonTK;
using PawnHistory.Source.PawnTracker;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.DebugTools;

public static class DebugOutputsHistoryRecords
{
    // Hot reload feature does not work with generic type
    // https://github.com/pardeike/Rimworld-Doorstop/issues/5
    public static Pawn[] AllPawns()
    {
        return Find.CurrentMap.mapPawns.AllPawns.ToArray();
    }
    public static Corpse[] AllCorpses()
    {
        return Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).Cast<Corpse>().ToArray();
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
                    new TableDataGetter<HistoryRecord>("Timestamp", r => r.date),
                    new TableDataGetter<HistoryRecord>("Date", r => compHistory.GetShortDate(r)),
                    new TableDataGetter<HistoryRecord>("label", r => r.def.label),
                    new TableDataGetter<HistoryRecord>("description", r => r.description),
                    new TableDataGetter<HistoryRecord>("concerns", r => string.Join(", ", r.concerns.Select(c =>
                    {
                        if (c == null) return "null";
                        if (c is Pawn p) return p.NameShortColored.Resolve();
                        return c.Label;
                    }))),
                    new TableDataGetter<HistoryRecord>("currentPawnToJumpTo", r => r.CurrentPawnToJumpTo)
                );
            }));
        }
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }
}
