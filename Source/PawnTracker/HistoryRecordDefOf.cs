using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace PawnHistory.Source.PawnTracker;

#pragma warning disable CS0649
[DefOf]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public class HistoryRecordDefOf
{
    public static HistoryRecordDef Raid;
    public static HistoryRecordDef SiteAmbush;
    public static HistoryRecordDef CaravanAmbush;
    public static HistoryRecordDef RaidFriendly;
    public static HistoryRecordDef Kill;
    public static HistoryRecordDef Death;
    public static HistoryRecordDef RelativeDeath;
    public static HistoryRecordDef BondedAnimalDeath;
    public static HistoryRecordDef Downed;
    public static HistoryRecordDef Crushed;
    public static HistoryRecordDef FriendlyTrapHit;
    public static HistoryRecordDef Anesthetized;
    public static HistoryRecordDef FoodPoisoning;
    public static HistoryRecordDef HealthComplication;
    public static HistoryRecordDef DrugAddicted;
    public static HistoryRecordDef Birthday;
    public static HistoryRecordDef SkillLeveledUp;
    public static HistoryRecordDef SkillLeveledDown;
    public static HistoryRecordDef Inspiration;
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
    public static HistoryRecordDef PlayerCaravanArrived;
    public static HistoryRecordDef PlayerTransporterArrived;
    public static HistoryRecordDef AICoreOffer;
    public static HistoryRecordDef PeaceTalksOutcome;
    public static HistoryRecordDef PeaceTalksRaid;
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
    public static HistoryRecordDef Kidnap;
    public static HistoryRecordDef Kidnapped;
    public static HistoryRecordDef JoinedParty;
    public static HistoryRecordDef PartyCancelled;
    public static HistoryRecordDef Disease;
    public static HistoryRecordDef LungRot;
    public static HistoryRecordDef MentalBreak;
    public static HistoryRecordDef MentalBreakViolent;
    public static HistoryRecordDef AnimalRevenge;
    public static HistoryRecordDef PredatorHuntingColonist;
    public static HistoryRecordDef SocialFight;
    public static HistoryRecordDef Rescued;
    public static HistoryRecordDef NewLover;
    public static HistoryRecordDef NewAffair;
    public static HistoryRecordDef Breakup;
    public static HistoryRecordDef MarriageProposal;
    public static HistoryRecordDef PrisonerCaptured;
    public static HistoryRecordDef PrisonerRecruited;
    public static HistoryRecordDef PrisonBreak;
    public static HistoryRecordDef LightningStrike;
    public static HistoryRecordDef WalkNaked;
    public static HistoryRecordDef ReadBook;
    public static HistoryRecordDef TechprintApplied;
    public static HistoryRecordDef Stripped;
    public static HistoryRecordDef CraftedQualityThing;
    public static HistoryRecordDef MinedValuable;
    public static HistoryRecordDef LongRangeMineralFound;
    public static HistoryRecordDef DeepMineralFound;
    public static HistoryRecordDef VisitedGrave;
    public static HistoryRecordDef Hunted;

    static HistoryRecordDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(HistoryRecordDefOf));
}
#pragma warning restore CS0649
