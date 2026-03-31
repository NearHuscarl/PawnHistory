using LudeonTK;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker;
using PawnHistory.Source.PawnTracker.Recorders;
using RimWorld;
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
        var allPawns = PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead.Where(RecorderManager.ShouldRecord)
            .OrderByDescending(p => p.GetHistoryRecords().Count)
            .ThenByDescending(p => p.GetHistoryRecords().LastOrDefault()?.date ?? 0);

        foreach (var pawn in allPawns)
        {
            var historyRecords = pawn.GetHistoryRecords();
            var label = $"{pawn.Name} ({historyRecords.Count})";

            options.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, () =>
            {
                DebugTables.MakeTablesDialog(historyRecords,
                    new TableDataGetter<HistoryRecord>("Timestamp", r => r.date),
                    new TableDataGetter<HistoryRecord>("Date", r => r.GetShortDate()),
                    new TableDataGetter<HistoryRecord>("label", r => r.def.label),
                    new TableDataGetter<HistoryRecord>("description", r => LangUtility.Truncate(r.description, 200)),
                    new TableDataGetter<HistoryRecord>("concerns", r => string.Join(", ", r.concerns.Select(c =>
                    {
                        if (c == null) return "null";
                        if (c is Pawn p) return p.NameDef();
                        return c.Label;
                    }))),
                    new TableDataGetter<HistoryRecord>("currentPawnToJumpTo", r => r.CurrentPawnToJumpTo),
                    new TableDataGetter<HistoryRecord>("tileId", r => r.tileId)
                );
            }));
        }
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }
}
