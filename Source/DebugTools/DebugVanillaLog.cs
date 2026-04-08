using LudeonTK;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Recorders;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static RimWorld.ITab_Pawn_Log_Utility;

namespace PawnHistory.Source.DebugTools;

public static class DebugVanillaLog
{
    private static List<LogEntry> GetVanillaLogs(Pawn pawn)
    {
        var logs = GenerateLogLinesFor(pawn, true, true, true, 300);
        var res = new List<LogEntry>();

        foreach (var log in logs)
        {
            if (log is LogLineDisplayableLog logLine)
                res.Add(Accessor.LogLineDisplayableLog.Log(logLine));
        }

        return res;
    }

    [NearDebugOutput]
    public static void VanillaLogOf()
    {
        var menuOptions = new List<DebugMenuOption>();
        var pawns = Find.CurrentMap.mapPawns.AllPawnsSpawned.Where(RecorderManager.ShouldRecord).ToList();
        var options = pawns.Select(p => new { Pawn = p, Logs = GetVanillaLogs(p) })
            .Where(o => o.Logs.Any())
            .OrderByDescending(o => o.Logs.Last().Timestamp);

        foreach (var option in options)
        {
            var label = $"{option.Pawn.Name} ({option.Logs.Count()})";

            menuOptions.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, () =>
            {
                DebugTables.MakeTablesDialog(option.Logs,
                    new TableDataGetter<LogEntry>("Timestamp", l => l.Timestamp),
                    new TableDataGetter<LogEntry>("Class", l => l.GetType().Name),
                    new TableDataGetter<LogEntry>("Text", l => l.ToGameStringFromPOV(option.Pawn)),
                    new TableDataGetter<LogEntry>("Concerns", l => l.GetConcerns().JoinToString())
                );
            }));
        }
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(menuOptions));
    }
}
