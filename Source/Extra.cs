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
    public static class RulePackDefOf
    {
        public static RulePackDef PH_Var;

        static RulePackDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(RulePackDefOf));
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
        [MayRequireRoyalty]
        public static PawnKindDef Empire_Fighter_Champion;
        public static PawnKindDef Tribal_Archer;
        public static PawnKindDef Tribal_Berserker;
        public static PawnKindDef Tribal_HeavyArcher;

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

        static RecipeDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(RecipeDefOf));
    }

    [DefOf]
    public static class ThingDefOf
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

        [MayRequireOdyssey]
        public static ThingDef AncientUplink;
        
        [MayRequireRoyalty]
        public static ThingDef MeleeWeapon_MonoSwordBladelink;

        static ThingDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }

    [DefOf]
    public static class RitualOutcomeEffectDefOf
    {
        public static RitualOutcomeEffectDef AttendedSpeech;

        static RitualOutcomeEffectDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(RitualOutcomeEffectDefOf));
    }

    [DefOf]
    public static class BackstoryDefOf
    {
        public static BackstoryDef MusicalKid86;
        public static BackstoryDef NavyScientist52;

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
        public static GeneDef PsychicBonding;
    }
}
