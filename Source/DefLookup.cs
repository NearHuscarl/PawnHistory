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
    }

    public static class BodyPart
    {
        public static BodyPartDef Brain => field ??= DefDatabase<BodyPartDef>.GetNamed("Brain");
        public static BodyPartDef Ear => field ??= DefDatabase<BodyPartDef>.GetNamed("Ear");
        public static BodyPartDef Spine => field ??= DefDatabase<BodyPartDef>.GetNamed("Spine");
    }

    public static class Interaction
    {
        public static InteractionDef Breakup => field ??= DefDatabase<InteractionDef>.GetNamed("Breakup");
    }

    public static class Incident
    {
        public static IncidentDef Disease_OrganDecay => field ??= DefDatabase<IncidentDef>.GetNamed("Disease_OrganDecay");
        public static IncidentDef Disease_Malaria => field ??= DefDatabase<IncidentDef>.GetNamed("Disease_Malaria");
        public static IncidentDef Disease_SleepingSickness => field ??= DefDatabase<IncidentDef>.GetNamed("Disease_SleepingSickness");
        public static IncidentDef Disease_SensoryMechanites => field ??= DefDatabase<IncidentDef>.GetNamed("Disease_SensoryMechanites");
    }

    public static class RaidStrategy
    {
        public static RaidStrategyDef Siege => field ??= DefDatabase<RaidStrategyDef>.GetNamed("Siege");
    }

    public static class PawnKind
    {
        public static PawnKindDef Husky => field ??= DefDatabase<PawnKindDef>.GetNamed("Husky");
    }
}