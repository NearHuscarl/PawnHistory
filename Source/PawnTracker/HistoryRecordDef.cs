using Verse;

namespace PawnHistory.Source.PawnTracker;

public enum RecordImportance
{
    Minor,
    Major,
}

public class HistoryRecordDef : Def
{
    public string icon;
    public RulePackDef descriptionMaker;
    public RecordImportance importance = RecordImportance.Major;
}

public static class HistoryRecordDefExtension
{
    public static HistoryDescriptionBuilder Description(this HistoryRecordDef recordDef, string rootKeyword, Pawn pawn)
    {
        return new HistoryDescriptionBuilder(recordDef, rootKeyword, pawn);
    }

    public static HistoryDescriptionBuilder ResolveDescription(this HistoryRecordDef recordDef, Pawn pawn)
    {
        return new HistoryDescriptionBuilder(recordDef, null, pawn);
    }
}
