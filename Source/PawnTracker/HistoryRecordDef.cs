using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public enum RecordImportance
{
    Minor,
    Major,
    Debug,
}

public class HistoryRecordDef : Def
{
    public string icon;
    public RulePackDef descriptionMaker;
    public RecordImportance importance = RecordImportance.Major;
    public List<HistoryRecordCategory> categories;
}

public static class HistoryRecordDefExtension
{
    public static HistoryDescriptionBuilder Description(this HistoryRecordDef recordDef, Pawn pawn, string keyword = null)
    {
        return new HistoryDescriptionBuilder(recordDef, pawn, keyword);
    }
}
