using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;
// ReSharper disable UnassignedField.Global

namespace PawnHistory.Source;

/// <summary>
/// Adds missing RimWorld <c>[DefOf]</c> entries used by the mod.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class DefLookup
{
    [DefOf]
    public static class RulePack
    {
        public static RulePackDef PH_Var;

        static RulePack() => DefOfHelper.EnsureInitializedInCtor(typeof(RulePack));
    }

    [DefOf]
    public static class Hediff
    {
        // ReSharper disable once IdentifierTypo
        public static HediffDef Alzheimers;
        public static HediffDef Asthma;
        public static HediffDef BadBack;
        public static HediffDef Frail;
        public static HediffDef HeartArteryBlockage;
        public static HediffDef WakeUpTolerance;
        public static HediffDef AlcoholTolerance;
        public static HediffDef ArchotechArm;
        public static HediffDef ArchotechEye;
        public static HediffDef BionicEar;
        public static HediffDef BionicSpine;
        public static HediffDef SmokeleafHigh;
        public static HediffDef Bruise;
        public static HediffDef Painstopper;
        public static HediffDef GoJuiceHigh;
        public static HediffDef HeartAttack;
        public static HediffDef LuciferiumAddiction;
        public static HediffDef SimpleProstheticHeart;
        public static HediffDef BionicHeart;

        static Hediff() => DefOfHelper.EnsureInitializedInCtor(typeof(Hediff));
    }

    [DefOf]
    public static class BodyPart
    {
        public static BodyPartDef Brain;
        public static BodyPartDef Ear;
        public static BodyPartDef Spine;
        public static BodyPartDef Nose;
        public static BodyPartDef Foot;
        public static BodyPartDef Kidney;

        static BodyPart() => DefOfHelper.EnsureInitializedInCtor(typeof(BodyPart));
    }

    [DefOf]
    public static class Interaction
    {
        public static InteractionDef Breakup;

        static Interaction() => DefOfHelper.EnsureInitializedInCtor(typeof(Interaction));
    }

    [DefOf]
    public static class MentalBreak
    {
        public static MentalBreakDef Slaughterer;
        public static MentalBreakDef Jailbreaker;
        public static MentalBreakDef SadisticRage;

        static MentalBreak() => DefOfHelper.EnsureInitializedInCtor(typeof(MentalBreak));
    }

    [DefOf]
    public static class Incident
    {
        public static IncidentDef Ambush;
        public static IncidentDef Disease_OrganDecay;
        public static IncidentDef Disease_Malaria;
        public static IncidentDef Disease_SleepingSickness;
        public static IncidentDef Disease_SensoryMechanites;
        public static IncidentDef StrangerInBlackJoin;
        public static IncidentDef RefugeePodCrash;
        public static IncidentDef WandererJoin;
        public static IncidentDef WildManWandersIn;
        public static IncidentDef GiveQuest_EndGame_ShipEscape;
        public static IncidentDef RansomDemand;

        static Incident() => DefOfHelper.EnsureInitializedInCtor(typeof(Incident));
    }

    [DefOf]
    public static class RaidStrategy
    {
        public static RaidStrategyDef Siege;

        static RaidStrategy() => DefOfHelper.EnsureInitializedInCtor(typeof(RaidStrategy));
    }

    [DefOf]
    public static class PawnKind
    {
        public static PawnKindDef Husky;
        public static PawnKindDef Cougar;
        public static PawnKindDef Bear_Grizzly;

        static PawnKind() => DefOfHelper.EnsureInitializedInCtor(typeof(PawnKind));
    }

    [DefOf]
    public static class TraderKind
    {
        public static TraderKindDef Caravan_Neolithic_Slaver;
        public static TraderKindDef Orbital_PirateMerchant;

        static TraderKind() => DefOfHelper.EnsureInitializedInCtor(typeof(TraderKind));
    }

    [DefOf]
    public static class QuestScript
    {
        public static QuestScriptDef TradeRequest;
        public static QuestScriptDef ThreatReward_Raid_Joiner;
        public static QuestScriptDef OpportunitySite_BanditCamp;
        public static QuestScriptDef OpportunitySite_PeaceTalks;
        public static QuestScriptDef OpportunitySite_DownedRefugee;

        [MayRequireRoyalty]
        public static QuestScriptDef Intro_Deserter;
        [MayRequireRoyalty]
        public static QuestScriptDef Intro_Wimp;
        [MayRequireRoyalty]
        public static QuestScriptDef ThreatReward_Infestation_Joiner;
        [MayRequireRoyalty]
        public static QuestScriptDef ThreatReward_Manhunters_Joiner;
        [MayRequireRoyalty]
        public static QuestScriptDef ThreatReward_GameCondition_Joiner;
        [MayRequireRoyalty]
        public static QuestScriptDef ThreatReward_SiteThreat_Joiner;
        [MayRequireRoyalty]
        public static QuestScriptDef ThreatReward_RaidMultiFaction_Joiner;
        [MayRequireRoyalty]
        public static QuestScriptDef ThreatReward_MysteryThreat_Joiner;
        [MayRequireRoyalty]
        public static QuestScriptDef WandererJoinAbasia;
        [MayRequireRoyalty]
        public static QuestScriptDef ShuttleCrash_Rescue;
        [MayRequireRoyalty]
        public static QuestScriptDef Hospitality_Animals;
        [MayRequireRoyalty]
        public static QuestScriptDef Hospitality_Joiners;
        [MayRequireRoyalty]
        public static QuestScriptDef Hospitality_Prisoners;
        [MayRequireRoyalty]
        public static QuestScriptDef EndGame_RoyalAscent;

        [MayRequireBiotech]
        public static QuestScriptDef SanguophageMeetingHost;

        static QuestScript() => DefOfHelper.EnsureInitializedInCtor(typeof(QuestScript));
    }

    [DefOf]
    public static class RoyalTitle
    {
        [MayRequireRoyalty]
        public static RoyalTitleDef Praetor;

        static RoyalTitle() => DefOfHelper.EnsureInitializedInCtor(typeof(RoyalTitle));
    }

    [DefOf]
    public static class Tale
    {
        public static TaleDef PlayedGame;
        public static TaleDef Stripped;
        public static TaleDef VisitedGrave;

        static Tale() => DefOfHelper.EnsureInitializedInCtor(typeof(Tale));
    }

    [DefOf]
    public static class Recipe
    {
        public static RecipeDef InstallJoywire;
        public static RecipeDef InstallNaturalLung;
        public static RecipeDef InstallNaturalHeart;
        public static RecipeDef InstallNaturalKidney;
        public static RecipeDef InstallBionicArm;
        public static RecipeDef InstallBionicHeart;
        public static RecipeDef InstallSimpleProstheticHeart;

        static Recipe() => DefOfHelper.EnsureInitializedInCtor(typeof(Recipe));
    }

    [DefOf]
    public static class Thing
    {
        public static ThingDef PodLauncher;
        public static ThingDef HorseshoesPin;
        public static ThingDef HoopstoneRing;
        public static ThingDef ChessTable;
        public static ThingDef GameOfUrBoard;
        public static ThingDef PokerTable;
        public static ThingDef Weapon_GrenadeFrag;
        public static ThingDef LongRangeMineralScanner;
        public static ThingDef BionicArm;
        public static ThingDef BionicHeart;
        public static ThingDef SimpleProstheticHeart;
        public static ThingDef Lung;
        public static ThingDef Heart;
        public static ThingDef Kidney;
        public static ThingDef Joywire;
        public static ThingDef VanometricPowerCell;
        
        [MayRequireRoyalty]
        public static ThingDef MeleeWeapon_MonoSwordBladelink;

        static Thing() => DefOfHelper.EnsureInitializedInCtor(typeof(Thing));
    }

    [DefOf]
    public static class RitualOutcomeEffect
    {
        public static RitualOutcomeEffectDef AttendedSpeech;

        static RitualOutcomeEffect() => DefOfHelper.EnsureInitializedInCtor(typeof(RitualOutcomeEffect));
    }

    [DefOf]
    public static class Backstory
    {
        public static BackstoryDef MusicalKid86;
        public static BackstoryDef NavyScientist52;

        static Backstory() => DefOfHelper.EnsureInitializedInCtor(typeof(Backstory));
    }

    [DefOf]
    public static class Trait
    {
        public static TraitDef TorturedArtist;
        public static TraitDef Gourmand;

        static Trait() => DefOfHelper.EnsureInitializedInCtor(typeof(Trait));
    }
}
