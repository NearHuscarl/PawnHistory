using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Ui;

public static class AddRecordDialogUtility
{
    public static List<HistoryRecordDef> LoadHistoryRecordDefs()
    {
        return DefDatabase<HistoryRecordDef>.AllDefsListForReading
            .Where(def => def.importance != RecordImportance.Debug)
            .OrderBy(def => def == HistoryRecordDefOf.Custom ? 0 : 1)
            .ThenBy(def => def.LabelCap.RawText)
            .ToList();
    }

    public static List<Quest> LoadQuests()
    {
        return Find.QuestManager.QuestsListForReading
            .Where(quest => !quest.hidden)
            .Reverse()
            .ToList();
    }
}