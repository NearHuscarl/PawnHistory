using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

// ".TryStartInspiration("
public enum InspirationCause
{
    Unknown,
    HighMood,
    Psilocap,
    PsychicInspiration,
    Ritual,
    Speech,
    Trait,
}

public record InspirationEventData(InspirationCause Cause, Trait AffectedByTrait, Pawn Initiator, RitualPatternDef AffectedByRitual) : GameEventBase;

public record InspirationStartedEvent(Pawn Pawn, InspirationDef Inspiration, InspirationEventData Data) : GameEventBase;

internal static class InspirationContext
{
    public static Trait TickingTrait;
    public static RitualPatternDef TickingRitual;
}

[HarmonyPatch(typeof(InspirationHandler), nameof(InspirationHandler.TryStartInspiration))]
internal static class InspirationHandler_TryStartInspiration_Patch
{
    private const string LetterInspirationBeginPsilocap = "LetterInspirationBeginPsilocap";
    private const string LetterInspirationBeginThanksToHighMoodPart = "LetterInspirationBeginThanksToHighMoodPart";
    private const string LetterPsychicInspiration = "LetterPsychicInspiration";
    private const string LetterSpeechInspiration = "LetterSpeechInspiration";

    public static void Postfix(bool __result, InspirationHandler __instance, InspirationDef def, string reason)
    {
        if (!__result)
            return;
        
        var cause = InspirationCause.Unknown;

        if (InspirationContext.TickingRitual != null)
            cause = InspirationCause.Ritual;
        else if (InspirationContext.TickingTrait != null)
            cause = InspirationCause.Trait;
        else if (reason == LetterInspirationBeginPsilocap.TranslateSimple())
            cause = InspirationCause.Psilocap;
        else if (reason == LetterInspirationBeginThanksToHighMoodPart.TranslateSimple())
            cause = InspirationCause.HighMood;
        else if (reason.MatchesTranslationTemplate(LetterPsychicInspiration, exactMatch: true))
            cause = InspirationCause.PsychicInspiration;
        else if (reason.MatchesTranslationTemplate(LetterSpeechInspiration, exactMatch: true))
            cause = InspirationCause.Speech;

        var eventData = new InspirationEventData(cause, InspirationContext.TickingTrait, Initiator: null /* TODO: DLCs */, InspirationContext.TickingRitual);
        GameEventBus.Publish(new InspirationStartedEvent(__instance.pawn, def, eventData));
    }
}

[HarmonyPatch(typeof(Trait), nameof(Trait.Notify_MentalStateEndedOn), typeof(Pawn))]
internal static class Trait_Notify_MentalStateEndedOn_Patch
{
    private static void Prefix(Trait __instance) => InspirationContext.TickingTrait = __instance;
    private static void Finalizer() => InspirationContext.TickingTrait = null;
}

[HarmonyPatch(typeof(RitualOutcomeEffectWorker_Consumable), "ApplyExtraOutcome")]
internal static class RitualOutcomeEffectWorker_Consumable_ApplyExtraOutcome_Patch
{
    private static void Prefix(LordJob_Ritual jobRitual) => InspirationContext.TickingRitual = jobRitual.Ritual.sourcePattern;
    private static void Finalizer() => InspirationContext.TickingRitual = null;
}
