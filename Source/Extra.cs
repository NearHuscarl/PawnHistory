using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;
// ReSharper disable UnassignedField.Global

namespace PawnHistory.Source;

/// <summary>
/// Fallback container for RimWorld <c>[DefOf]</c> entries the mod needs beyond the built-in classes.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class Extra
{
    [DefOf]
    public static class AbilityDefOf
    {
        [MayRequireIdeology]
        public static AbilityDef Convert;
        [MayRequireIdeology]
        public static AbilityDef ConversionRitual;
        [MayRequireIdeology]
        public static AbilityDef LeaderSpeech;

        static AbilityDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(AbilityDefOf));
    }

    [DefOf]
    public static class RulePackDefOf
    {
        public static RulePackDef PH_Var;

        static RulePackDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(RulePackDefOf));
    }

    [DefOf]
    public static class GatheringDefOf
    {
        [MayRequireRoyalty]
        public static GatheringDef Concert;

        static GatheringDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(GatheringDefOf));
    }

    [DefOf]
    public static class HediffDefOf
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
        public static HediffDef LuciferiumHigh;
        [MayRequireBiotech]
        public static HediffDef ChildbirthComplications;

        static HediffDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
    }

    [DefOf]
    public static class BodyPartDefOf
    {
        public static BodyPartDef Brain;
        public static BodyPartDef Ear;
        public static BodyPartDef Spine;
        public static BodyPartDef Nose;
        public static BodyPartDef Foot;
        public static BodyPartDef Kidney;
        public static BodyPartDef Jaw;

        static BodyPartDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(BodyPartDefOf));
    }

    [DefOf]
    public static class InteractionDefOf
    {
        public static InteractionDef Breakup;

        static InteractionDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(InteractionDefOf));
    }

    [DefOf]
    public static class MentalBreakDefOf
    {
        public static MentalBreakDef Binging_DrugMajor;
        public static MentalBreakDef Binging_DrugExtreme;
        public static MentalBreakDef Slaughterer;
        public static MentalBreakDef Jailbreaker;
        public static MentalBreakDef SadisticRage;
        public static MentalBreakDef RunWild;
        public static MentalBreakDef TargetedTantrum;
        
        [MayRequireRoyalty]
        public static MentalBreakDef WildDecree;
        
        [MayRequireIdeology]
        public static MentalBreakDef IdeoChange;
        [MayRequireIdeology]
        public static MentalBreakDef Rebellion;

        static MentalBreakDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(MentalBreakDefOf));
    }

    [DefOf]
    public static class IncidentDefOf
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

        static IncidentDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf));
    }

    [DefOf]
    public static class RaidStrategyDefOf
    {
        public static RaidStrategyDef Siege;

        static RaidStrategyDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(RaidStrategyDefOf));
    }

    [DefOf]
    public static class PawnKindDefOf
    {
        public static PawnKindDef Mercenary_Sniper_Acidifier;
        public static PawnKindDef Mercenary_Gunner_Acidifier;
        public static PawnKindDef Mercenary_Slasher_Acidifier;
        public static PawnKindDef Mercenary_Elite_Acidifier;
        public static PawnKindDef Tribal_Warrior;
        public static PawnKindDef Husky;
        public static PawnKindDef Cougar;
        public static PawnKindDef Bear_Grizzly;
        public static PawnKindDef Tribal_Archer;
        public static PawnKindDef Tribal_Berserker;
        public static PawnKindDef Tribal_HeavyArcher;

        [MayRequireRoyalty]
        public static PawnKindDef Empire_Fighter_Champion;
        [MayRequireRoyalty]
        public static PawnKindDef Empire_Fighter_StellicGuardRanged;
        [MayRequireRoyalty]
        public static PawnKindDef Empire_Fighter_StellicGuardMelee;
        [MayRequireRoyalty]
        public static PawnKindDef Empire_Royal_Stellarch;

        static PawnKindDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf));
    }

    [DefOf]
    public static class TraderKindDefOf
    {
        public static TraderKindDef Caravan_Neolithic_Slaver;
        public static TraderKindDef Orbital_PirateMerchant;

        static TraderKindDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(TraderKindDefOf));
    }

    [DefOf]
    public static class QuestScriptDefOf
    {
        public static QuestScriptDef TradeRequest;
        public static QuestScriptDef ThreatReward_Raid_Joiner;
        public static QuestScriptDef OpportunitySite_DownedRefugee;
        public static QuestScriptDef OpportunitySite_BanditCamp;
        public static QuestScriptDef OpportunitySite_PeaceTalks;
        public static QuestScriptDef OpportunitySite_PrisonerWillingToJoin;

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
        public static QuestScriptDef ThreatReward_Raid_MiscReward;
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
        public static QuestScriptDef PawnLend;
        [MayRequireRoyalty]
        public static QuestScriptDef Mission_BanditCamp;
        [MayRequireRoyalty]
        public static QuestScriptDef EndGame_RoyalAscent;

        [MayRequireBiotech]
        public static QuestScriptDef SanguophageMeetingHost;
        [MayRequireBiotech]
        public static QuestScriptDef RefugeePodCrash_Baby;

        [MayRequireIdeology]
        public static QuestScriptDef Beggars;
        [MayRequireIdeology]
        public static QuestScriptDef OpportunitySite_WorkSite;

        static QuestScriptDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(QuestScriptDefOf));
    }

    [DefOf]
    public static class RoyalTitleDefOf
    {
        [MayRequireRoyalty]
        public static RoyalTitleDef Praetor;

        static RoyalTitleDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(RoyalTitleDefOf));
    }

    [DefOf]
    public static class TaleDefOf
    {
        public static TaleDef PlayedGame;
        public static TaleDef Stripped;
        public static TaleDef VisitedGrave;

        static TaleDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(TaleDefOf));
    }

    [DefOf]
    public static class RecipeDefOf
    {
        public static RecipeDef InstallJoywire;
        public static RecipeDef InstallNaturalLung;
        public static RecipeDef InstallNaturalHeart;
        public static RecipeDef InstallNaturalKidney;
        public static RecipeDef InstallBionicArm;
        public static RecipeDef InstallBionicHeart;
        public static RecipeDef InstallSimpleProstheticHeart;
        public static RecipeDef InstallArchotechArm;
        public static RecipeDef InstallDenture;

        [MayRequireBiotech]
        public static RecipeDef TerminatePregnancy;
        [MayRequireBiotech]
        public static RecipeDef ImplantIUD;
        [MayRequireBiotech]
        public static RecipeDef RemoveIUD;
        [MayRequireBiotech]
        public static RecipeDef TubalLigation;
        [MayRequireBiotech]
        public static RecipeDef Vasectomy;
        [MayRequireBiotech]
        public static RecipeDef ReverseVasectomy;

        static RecipeDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(RecipeDefOf));
    }

    [DefOf]
    public static class ThingDefOf
    {
        public static ThingDef Apparel_PowerArmorHelmet;
        public static ThingDef Apparel_PowerArmor;
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
        public static ThingDef Harpsichord;
        [MayRequireRoyalty]
        public static ThingDef MeleeWeapon_MonoSwordBladelink;

        [MayRequireIdeology]
        public static ThingDef Burnbong;
        [MayRequireIdeology]
        public static ThingDef CannibalPlatter;
        [MayRequireIdeology]
        public static ThingDef ChristmasTree;
        [MayRequireIdeology]
        public static ThingDef Effigy;

        [MayRequireOdyssey]
        public static ThingDef AncientUplink;
        
        static ThingDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }

    [DefOf]
    public static class RitualOutcomeEffectDefOf
    {
        public static RitualOutcomeEffectDef AttendedSpeech;
        [MayRequireIdeology]
        public static RitualOutcomeEffectDef CelebratedDate;
        [MayRequireIdeology]
        public static RitualOutcomeEffectDef AttendedFuneral;
        [MayRequireIdeology]
        public static RitualOutcomeEffectDef AttendedFuneralNoCorpse;
        [MayRequireIdeology]
        public static RitualOutcomeEffectDef Conversion;
        [MayRequireIdeology]
        public static RitualOutcomeEffectDef Execution;
        [MayRequireIdeology]
        public static RitualOutcomeEffectDef Trial;
        [MayRequireIdeology]
        public static RitualOutcomeEffectDef CelebrationSkyLanterns;
        [MayRequireIdeology]
        public static RitualOutcomeEffectDef RoleChange;
        [MayRequireIdeology]
        public static RitualOutcomeEffectDef BlindingCeremony;
        [MayRequireIdeology]
        public static RitualOutcomeEffectDef ScarificationCeremony;
        [MayRequireIdeology]
        public static RitualOutcomeEffectDef GladiatorDuel;
        [MayRequireIdeology]
        public static RitualOutcomeEffectDef Sacrifice;

        static RitualOutcomeEffectDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(RitualOutcomeEffectDefOf));
    }

    [DefOf]
    public static class RitualPatternDefOf
    {
        [MayRequireIdeology]
        public static RitualPatternDef BurnCircle;
        [MayRequireIdeology]
        public static RitualPatternDef CelebrationSkyLanterns;
        [MayRequireIdeology]
        public static RitualPatternDef FeastCannibal;
        [MayRequireIdeology]
        public static RitualPatternDef SmokeCircle;
        [MayRequireIdeology]
        public static RitualPatternDef SacrificePrisoner;
        [MayRequireIdeology]
        public static RitualPatternDef SacrificeAnimal;

        static RitualPatternDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(RitualPatternDefOf));
    }

    [DefOf]
    public static class BackstoryDefOf
    {
        public static BackstoryDef MusicalKid86;
        public static BackstoryDef NavyScientist52;
        public static BackstoryDef TribeChild19;

        static BackstoryDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(BackstoryDefOf));
    }

    [DefOf]
    public static class TraitDefOf
    {
        public static TraitDef TorturedArtist;
        public static TraitDef Gourmand;

        static TraitDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(TraitDefOf));
    }

    [DefOf]
    public static class GeneDefOf
    {
        [MayRequireBiotech]
        public static GeneDef Deathrest;
        [MayRequireBiotech]
        public static GeneDef PsychicBonding;
        [MayRequireBiotech]
        public static GeneDef TotalHealing;
    }

    [DefOf]
    public static class XenotypeDefOf
    {
        [MayRequireBiotech]
        public static XenotypeDef Dirtmole;
    }

    [DefOf]
    public static class XenotypeIconDefOf
    {
        [MayRequireBiotech]
        public static XenotypeIconDef Crown;
    }

    [DefOf]
    public static class PreceptDefOf
    {
        [MayRequireIdeology]
        public static PreceptDef SpouseCount_Female_Unlimited;
        [MayRequireIdeology]
        public static PreceptDef Bonding_Disapproved;
        [MayRequireIdeology]
        public static PreceptDef Conversion;
        [MayRequireIdeology]
        public static PreceptDef Execution;
        [MayRequireIdeology]
        public static PreceptDef Trial;
        [MayRequireIdeology]
        public static PreceptDef TrialPrisoner;
        [MayRequireIdeology]
        public static PreceptDef TrialMentalState;
        [MayRequireIdeology]
        public static PreceptDef TreeConnection;
        [MayRequireIdeology]
        public static PreceptDef Festival;
        [MayRequireIdeology]
        public static PreceptDef DateRitualConsumable;
        [MayRequireIdeology]
        public static PreceptDef Classic_DrumParty;
        [MayRequireIdeology]
        public static PreceptDef Classic_DanceParty;
        [MayRequireIdeology]
        public static PreceptDef LeaderSpeech;
        [MayRequireIdeology]
        public static PreceptDef BlindingCeremony;
        [MayRequireIdeology]
        public static PreceptDef Blindness_Respected;
        [MayRequireIdeology]
        public static PreceptDef ScarificationCeremony;
        [MayRequireIdeology]
        public static PreceptDef Scarification_Minor;
        [MayRequireIdeology]
        public static PreceptDef GladiatorDuel;
    }
}
