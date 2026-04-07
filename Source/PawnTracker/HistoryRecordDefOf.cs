using RimWorld;

namespace PawnHistory.Source.PawnTracker;

#pragma warning disable CS0649
[DefOf]
public class HistoryRecordDefOf
{
    public static HistoryRecordDef Raid;
    public static HistoryRecordDef RaidFriendly;
    public static HistoryRecordDef Kill;
    public static HistoryRecordDef Death;
    public static HistoryRecordDef RelativeDeath;
    public static HistoryRecordDef Downed;
    public static HistoryRecordDef Anesthetized;
    public static HistoryRecordDef FoodPoisoning;
    public static HistoryRecordDef HealthComplication;
    public static HistoryRecordDef Birthday;
    public static HistoryRecordDef SkillLeveledUp;
    public static HistoryRecordDef SkillLeveledDown;
    public static HistoryRecordDef BodyPartDestroyed;
    public static HistoryRecordDef BodyPartScarred;
    public static HistoryRecordDef BodyPartRemoved;
    public static HistoryRecordDef BodyPartImplanted;
    public static HistoryRecordDef BodyPartInstalled;
    public static HistoryRecordDef BodyPartModded;
    public static HistoryRecordDef BotchedSurgery;
    public static HistoryRecordDef NewArrival;
    public static HistoryRecordDef LeaderChanged;
    public static HistoryRecordDef AncientDangerWarning;
    public static HistoryRecordDef CasketDrop;
    public static HistoryRecordDef CasketAwakened;
    public static HistoryRecordDef TradeCaravanArrived;
    public static HistoryRecordDef TradeCaravanLeft;
    public static HistoryRecordDef VisitorArrived;
    public static HistoryRecordDef TravelGroupArrived;
    public static HistoryRecordDef RescueJoined;
    public static HistoryRecordDef WandererJoined;
    public static HistoryRecordDef RefugeePodCrashed;
    public static HistoryRecordDef WildManWanderedIn;
    public static HistoryRecordDef ManInBlackJoin;
    public static HistoryRecordDef GameEndedWanderersJoined;
    public static HistoryRecordDef SoldToSlavery;
    public static HistoryRecordDef BoughtFromSlavery;
    public static HistoryRecordDef JoinedParty;
    public static HistoryRecordDef PartyCancelled;
    public static HistoryRecordDef Disease;
    public static HistoryRecordDef MentalBreak;
    public static HistoryRecordDef MentalBreakViolent;
    public static HistoryRecordDef SocialFight;
    public static HistoryRecordDef Rescued;
    public static HistoryRecordDef PrisonerCaptured;
    public static HistoryRecordDef PrisonerRecruited;
    public static HistoryRecordDef PrisonBreak;
    public static HistoryRecordDef LightningStriked;
    public static HistoryRecordDef WalkNaked;
    public static HistoryRecordDef ReadBook;
    public static HistoryRecordDef Stripped;
    public static HistoryRecordDef MinedValuable;
    public static HistoryRecordDef VisitedGrave;
    public static HistoryRecordDef Hunted;

    static HistoryRecordDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(HistoryRecordDefOf));
}
#pragma warning restore CS0649