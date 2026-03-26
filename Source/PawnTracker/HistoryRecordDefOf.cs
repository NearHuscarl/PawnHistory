using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker;

#pragma warning disable CS0649
[DefOf]
internal class HistoryRecordDefOf
{
    public static HistoryRecordDef Raid;
    public static HistoryRecordDef RaidFriendly;
    public static HistoryRecordDef Kill;
    public static HistoryRecordDef Death;
    public static HistoryRecordDef RelativeDeath;
    public static HistoryRecordDef Downed;
    public static HistoryRecordDef Anesthetized;
    public static HistoryRecordDef BodyPartDestroyed;
    public static HistoryRecordDef BodyPartScarred;
    public static HistoryRecordDef BodyPartRemoved;
    public static HistoryRecordDef BodyPartImplanted;
    public static HistoryRecordDef BodyPartInstalled;
    public static HistoryRecordDef BodyPartModded;
    public static HistoryRecordDef BotchedSurgery;
    public static HistoryRecordDef TradeCaravanArrived;
    public static HistoryRecordDef TradeCaravanLeft;
    public static HistoryRecordDef VisitorArrived;
    public static HistoryRecordDef TravelGroupArrived;
    public static HistoryRecordDef ManInBlackJoin;
    public static HistoryRecordDef MentalBreak;
    public static HistoryRecordDef MentalBreakViolent;
    public static HistoryRecordDef SocialFight;
    public static HistoryRecordDef PrisonBreak;
    public static HistoryRecordDef LightningStriked;
    public static HistoryRecordDef WalkNaked;
    public static HistoryRecordDef ReadBook;
    public static HistoryRecordDef Stripped;

    static HistoryRecordDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(HistoryRecordDefOf));
}
#pragma warning restore CS0649