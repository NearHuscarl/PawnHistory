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
    public static void HistoryRecordDefs()
    {
        DebugTables.MakeTablesDialog(DefDatabase<HistoryRecordDef>.AllDefs,
            new TableDataGetter<HistoryRecordDef>("defName", d => d.defName),
            new TableDataGetter<HistoryRecordDef>("label", d => d.label),
            new TableDataGetter<HistoryRecordDef>("categories", d => d.categories.JoinToString()),
            new TableDataGetter<HistoryRecordDef>("importance", d => d.importance),
            new TableDataGetter<HistoryRecordDef>("icon", d => d.icon)
        );
    }

    [NearDebugOutput]
    public static void PawnHistoryRecords()
    {
        var options = new List<DebugMenuOption>();
        var allPawns = PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead.Where(RecorderManager.ShouldRecord)
            .OrderByDescending(p => p.HistoryRecords.Count)
            .ThenByDescending(p => p.HistoryRecords.LastOrDefault()?.date ?? 0);

        foreach (var pawn in allPawns)
        {
            var historyRecords = pawn.HistoryRecords;
            var label = $"{pawn.Name} ({historyRecords.Count})";

            options.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, () =>
            {
                DebugTables.MakeTablesDialog(historyRecords,
                    new TableDataGetter<HistoryRecord>("Timestamp", r => r.date),
                    new TableDataGetter<HistoryRecord>("Date", r => r.GetShortDate()),
                    new TableDataGetter<HistoryRecord>("Def", r => r.def.defName),
                    new TableDataGetter<HistoryRecord>("Description", r => LangUtility.Truncate(r.description, 200)),
                    new TableDataGetter<HistoryRecord>("Concerns", r => r.ConcernedThings.Select(c =>
                    {
                        if (c == null) return "null";
                        if (c is Pawn p) return p.NameDef;
                        return c.Label;
                    }).JoinToString()),
                    new TableDataGetter<HistoryRecord>("Position", r => r.location == null ? "" : $"{r.location.map} {r.location.position}"),
                    new TableDataGetter<HistoryRecord>("tileId", r => r.tileId),
                    new TableDataGetter<HistoryRecord>("currentPawnToJumpTo", r => r.CurrentPawnToJumpTo)
                );
            }));
        }
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }
}
