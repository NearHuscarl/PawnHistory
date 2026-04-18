using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace PawnHistory.Source;

/// <summary>
/// Extends any missing RimWorld <c>[DefOf]</c> class but defs are not covered in XML hot reload.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class DefLookup
{
    public static class RulePack
    {
        public static RulePackDef PH_Var => field ??= DefDatabase<RulePackDef>.GetNamed("PH_Var");
    }

    public static class Hediff
    {
        // ReSharper disable once StringLiteralTypo
        // ReSharper disable once IdentifierTypo
        public static HediffDef Alzheimers => field ??= DefDatabase<HediffDef>.GetNamed("Alzheimers");
        public static HediffDef Asthma => field ??= DefDatabase<HediffDef>.GetNamed("Asthma");
        public static HediffDef BadBack => field ??= DefDatabase<HediffDef>.GetNamed("BadBack");
        public static HediffDef Frail => field ??= DefDatabase<HediffDef>.GetNamed("Frail");
        public static HediffDef HeartArteryBlockage => field ??= DefDatabase<HediffDef>.GetNamed("HeartArteryBlockage");
        public static HediffDef WakeUpTolerance => field ??= DefDatabase<HediffDef>.GetNamed("WakeUpTolerance");
        public static HediffDef AlcoholTolerance => field ??= DefDatabase<HediffDef>.GetNamed("AlcoholTolerance");
        public static HediffDef ArchotechArm => field ??= DefDatabase<HediffDef>.GetNamed("ArchotechArm");
        public static HediffDef SmokeleafHigh => field ??= DefDatabase<HediffDef>.GetNamed("SmokeleafHigh");
        public static HediffDef Bruise => field ??= DefDatabase<HediffDef>.GetNamed("Bruise");
        public static HediffDef Painstopper => field ??= DefDatabase<HediffDef>.GetNamed("Painstopper");
        public static HediffDef GoJuiceHigh => field ??= DefDatabase<HediffDef>.GetNamed("GoJuiceHigh");
        public static HediffDef HeartAttack => field ??= DefDatabase<HediffDef>.GetNamed("HeartAttack");
        public static HediffDef LuciferiumAddiction => field ??= DefDatabase<HediffDef>.GetNamed("LuciferiumAddiction");
        public static HediffDef SimpleProstheticHeart => field ??= DefDatabase<HediffDef>.GetNamed("SimpleProstheticHeart");
        public static HediffDef BionicHeart => field ??= DefDatabase<HediffDef>.GetNamed("BionicHeart");
    }

    public static class BodyPart
    {
        public static BodyPartDef Brain => field ??= DefDatabase<BodyPartDef>.GetNamed("Brain");
        public static BodyPartDef Ear => field ??= DefDatabase<BodyPartDef>.GetNamed("Ear");
        public static BodyPartDef Spine => field ??= DefDatabase<BodyPartDef>.GetNamed("Spine");
        public static BodyPartDef Nose => field ??= DefDatabase<BodyPartDef>.GetNamed("Nose");
        public static BodyPartDef Foot => field ??= DefDatabase<BodyPartDef>.GetNamed("Foot");
        public static BodyPartDef Kidney => field ??= DefDatabase<BodyPartDef>.GetNamed("Kidney");
    }

    public static class Interaction
    {
        public static InteractionDef Breakup => field ??= DefDatabase<InteractionDef>.GetNamed("Breakup");
    }

    public static class MentalBreak
    {
        public static MentalBreakDef Slaughterer => field ??= DefDatabase<MentalBreakDef>.GetNamed("Slaughterer");
        public static MentalBreakDef Jailbreaker => field ??= DefDatabase<MentalBreakDef>.GetNamed("Jailbreaker");
        public static MentalBreakDef SadisticRage => field ??= DefDatabase<MentalBreakDef>.GetNamed("SadisticRage");
    }

    public static class Incident
    {
        public static IncidentDef Ambush => field ??= DefDatabase<IncidentDef>.GetNamed("Ambush");
        public static IncidentDef Disease_OrganDecay => field ??= DefDatabase<IncidentDef>.GetNamed("Disease_OrganDecay");
        public static IncidentDef Disease_Malaria => field ??= DefDatabase<IncidentDef>.GetNamed("Disease_Malaria");
        public static IncidentDef Disease_SleepingSickness => field ??= DefDatabase<IncidentDef>.GetNamed("Disease_SleepingSickness");
        public static IncidentDef Disease_SensoryMechanites => field ??= DefDatabase<IncidentDef>.GetNamed("Disease_SensoryMechanites");
        public static IncidentDef StrangerInBlackJoin => field ??= DefDatabase<IncidentDef>.GetNamed("StrangerInBlackJoin");
        public static IncidentDef RefugeePodCrash => field ??= DefDatabase<IncidentDef>.GetNamed("RefugeePodCrash");
        public static IncidentDef WandererJoin => field ??= DefDatabase<IncidentDef>.GetNamed("WandererJoin");
        public static IncidentDef WildManWandersIn => field ??= DefDatabase<IncidentDef>.GetNamed("WildManWandersIn");
        public static IncidentDef GiveQuest_EndGame_ShipEscape => field ??= DefDatabase<IncidentDef>.GetNamed("GiveQuest_EndGame_ShipEscape");
    }

    public static class RaidStrategy
    {
        public static RaidStrategyDef Siege => field ??= DefDatabase<RaidStrategyDef>.GetNamed("Siege");
    }

    public static class PawnKind
    {
        public static PawnKindDef Husky => field ??= DefDatabase<PawnKindDef>.GetNamed("Husky");
        public static PawnKindDef Cougar => field ??= DefDatabase<PawnKindDef>.GetNamed("Cougar");
    }

    public static class TraderKind
    {
        public static TraderKindDef Caravan_Neolithic_Slaver => field ??= DefDatabase<TraderKindDef>.GetNamed("Caravan_Neolithic_Slaver");
    }

    public static class QuestScript
    {
        public static QuestScriptDef OpportunitySite_PeaceTalks => field ??= DefDatabase<QuestScriptDef>.GetNamed("OpportunitySite_PeaceTalks");
        public static QuestScriptDef OpportunitySite_DownedRefugee => field ??= DefDatabase<QuestScriptDef>.GetNamed("OpportunitySite_DownedRefugee");
    }

    public static class Tale
    {
        public static TaleDef Stripped => field ??= DefDatabase<TaleDef>.GetNamed("Stripped");
        public static TaleDef VisitedGrave => field ??= DefDatabase<TaleDef>.GetNamed("VisitedGrave");
    }

    public static class Recipe
    {
        public static RecipeDef InstallJoywire => field ??= DefDatabase<RecipeDef>.GetNamed("InstallJoywire");
        public static RecipeDef InstallNaturalLung => field ??= DefDatabase<RecipeDef>.GetNamed("InstallNaturalLung");
        public static RecipeDef InstallNaturalHeart => field ??= DefDatabase<RecipeDef>.GetNamed("InstallNaturalHeart");
        public static RecipeDef InstallNaturalKidney => field ??= DefDatabase<RecipeDef>.GetNamed("InstallNaturalKidney");
        public static RecipeDef InstallBionicArm => field ??= DefDatabase<RecipeDef>.GetNamed("InstallBionicArm");
        public static RecipeDef InstallBionicHeart => field ??= DefDatabase<RecipeDef>.GetNamed("InstallBionicHeart");
        public static RecipeDef InstallSimpleProstheticHeart => field ??= DefDatabase<RecipeDef>.GetNamed("InstallSimpleProstheticHeart");
    }

    public static class Thing
    {
        public static ThingDef PodLauncher => field ??= DefDatabase<ThingDef>.GetNamed("PodLauncher");
        public static ThingDef Weapon_GrenadeFrag => field ??= DefDatabase<ThingDef>.GetNamed("Weapon_GrenadeFrag");
        public static ThingDef LongRangeMineralScanner => field ??= DefDatabase<ThingDef>.GetNamed("LongRangeMineralScanner");
    }

    public static class Backstory
    {
        public static BackstoryDef MusicalKid86 => field ??= DefDatabase<BackstoryDef>.GetNamed("MusicalKid86");
        public static BackstoryDef NavyScientist52 => field ??= DefDatabase<BackstoryDef>.GetNamed("NavyScientist52");
    }

    public static class Trait
    {
        public static TraitDef TorturedArtist => field ??= DefDatabase<TraitDef>.GetNamed("TorturedArtist");
        public static TraitDef Gourmand => field ??= DefDatabase<TraitDef>.GetNamed("Gourmand");
    }
}
