using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace PawnHistory.Source.PawnTracker;

#pragma warning disable CS0649
[DefOf]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public class HistoryRecordDefOf
{
    public static HistoryRecordDef Custom;
    public static HistoryRecordDef PawnGenerated;
    public static HistoryRecordDef Raid;
    public static HistoryRecordDef RaidersLeft;
    public static HistoryRecordDef SiteAmbush;
    public static HistoryRecordDef CaravanAmbush;
    public static HistoryRecordDef DefenderGenerated;
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
    public static HistoryRecordDef ScarHealed;
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
    public static HistoryRecordDef VisitorLeftGift;
    public static HistoryRecordDef QuestPawnArrived;
    public static HistoryRecordDef LodgerJoinOffer;
    public static HistoryRecordDef PeaceTalksOutcome;
    public static HistoryRecordDef PeaceTalksRaid;
    public static HistoryRecordDef RitualOutcome;
    public static HistoryRecordDef TradeCaravanArrived;
    public static HistoryRecordDef TradeCaravanLeft;
    public static HistoryRecordDef VisitorArrived;
    public static HistoryRecordDef TravelGroupArrived;
    public static HistoryRecordDef RescueJoined;
    public static HistoryRecordDef OfferHelp;
    public static HistoryRecordDef WildManWanderedIn;
    public static HistoryRecordDef ManInBlackJoin;
    public static HistoryRecordDef GameEndedWanderersJoined;
    public static HistoryRecordDef SoldToSlavery;
    public static HistoryRecordDef BoughtFromSlavery;
    public static HistoryRecordDef Kidnap;
    public static HistoryRecordDef Kidnapped;
    public static HistoryRecordDef Ransom;
    public static HistoryRecordDef Banish;
    public static HistoryRecordDef PawnLost;
    public static HistoryRecordDef PartyAttended;
    public static HistoryRecordDef Married;
    public static HistoryRecordDef WeddingStarted;
    public static HistoryRecordDef WeddingJoined;
    public static HistoryRecordDef WeddingFinished;
    public static HistoryRecordDef Disease;
    public static HistoryRecordDef LungRot;
    public static HistoryRecordDef MentalBreak;
    public static HistoryRecordDef MentalBreakViolent;
    public static HistoryRecordDef AnimalRevenge;
    public static HistoryRecordDef AnimalTamed;
    public static HistoryRecordDef PredatorHuntingColonist;
    public static HistoryRecordDef SocialFight;
    public static HistoryRecordDef Rescued;
    public static HistoryRecordDef NewLover;
    public static HistoryRecordDef NewAffair;
    public static HistoryRecordDef Breakup;
    public static HistoryRecordDef MarriageProposal;
    public static HistoryRecordDef PrisonerCaptured;
    public static HistoryRecordDef PrisonerExecuted;
    public static HistoryRecordDef PrisonerReleased;
    public static HistoryRecordDef PrisonerRecruited;
    public static HistoryRecordDef PrisonerEscaped;
    public static HistoryRecordDef PrisonBreak;
    public static HistoryRecordDef LightningStrike;
    public static HistoryRecordDef WalkNaked;
    public static HistoryRecordDef OnFire;
    public static HistoryRecordDef Exhausted;
    public static HistoryRecordDef PlayedGame;
    public static HistoryRecordDef ReadBook;
    public static HistoryRecordDef Stripped;
    public static HistoryRecordDef CraftedQualityThing;
    public static HistoryRecordDef MinedValuable;
    public static HistoryRecordDef LongRangeMineralFound;
    public static HistoryRecordDef DeepMineralFound;
    public static HistoryRecordDef VisitedGrave;
    public static HistoryRecordDef Hunted;
    
    [MayRequireRoyalty]
    public static HistoryRecordDef TechprintApplied;
    [MayRequireRoyalty]
    public static HistoryRecordDef WeaponBonded;
    [MayRequireRoyalty]
    public static HistoryRecordDef PsylinkLevelGained;
    [MayRequireRoyalty]
    public static HistoryRecordDef TitleGained;
    [MayRequireRoyalty]
    public static HistoryRecordDef TitleLost;
    [MayRequireRoyalty]
    public static HistoryRecordDef TitleInherited;
    [MayRequireRoyalty]
    public static HistoryRecordDef QuestRefugeeBetrayalOffer;
    [MayRequireRoyalty]
    public static HistoryRecordDef QuestRefugeeAssault;
    [MayRequireRoyalty]
    public static HistoryRecordDef ConcertAttended;
    [MayRequireRoyalty]
    public static HistoryRecordDef ConcertHeld;

    [MayRequireBiotech]
    public static HistoryRecordDef MechlinkInstalled;
    [MayRequireBiotech]
    public static HistoryRecordDef XenogermImplanted;
    [MayRequireBiotech]
    public static HistoryRecordDef DeathrestOrComa;
    [MayRequireBiotech]
    public static HistoryRecordDef BabyAdopted;
    [MayRequireBiotech]
    public static HistoryRecordDef GrowthMoment;
    [MayRequireBiotech]
    public static HistoryRecordDef PregnancyStarted;
    [MayRequireBiotech]
    public static HistoryRecordDef PregnancyTerminated;
    [MayRequireBiotech]
    public static HistoryRecordDef Sterilized;
    [MayRequireBiotech]
    public static HistoryRecordDef ReverseVasectomy;
    [MayRequireBiotech]
    public static HistoryRecordDef IudImplanted;
    [MayRequireBiotech]
    public static HistoryRecordDef IudRemoved;
    [MayRequireBiotech]
    public static HistoryRecordDef GaveBirth;
    [MayRequireBiotech]
    public static HistoryRecordDef Miscarried;
    [MayRequireBiotech]
    public static HistoryRecordDef PsychicBonded;

    [MayRequireIdeology]
    public static HistoryRecordDef Enslaved;
    [MayRequireIdeology]
    public static HistoryRecordDef SlaveEmancipated;
    [MayRequireIdeology]
    public static HistoryRecordDef SlaveExecuted;
    [MayRequireIdeology]
    public static HistoryRecordDef IdeoChanged;
    [MayRequireIdeology]
    public static HistoryRecordDef IdeoRoleChanged;
    [MayRequireIdeology]
    public static HistoryRecordDef DivorceByIdeo;
    [MayRequireIdeology]
    public static HistoryRecordDef BondRemovedByIdeo;
    [MayRequireIdeology]
    public static HistoryRecordDef SlaveRebellion;

    [MayRequireOdyssey]
    public static HistoryRecordDef QuestDiscovered;

    static HistoryRecordDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(HistoryRecordDefOf));
}
#pragma warning restore CS0649
