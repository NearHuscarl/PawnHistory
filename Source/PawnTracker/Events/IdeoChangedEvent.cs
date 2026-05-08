using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public enum IdeoChangeReason
{
    Unknown,
    ConvertAbility,
    SocialInteraction,
    ConversionRitual,
    SpeechRitual,
    Exposure,
    MentalBreak,
}

public record IdeoChangedEvent(Pawn Pawn, Ideo OldIdeo, Ideo NewIdeo, IdeoChangeReason Reason, Pawn Converter = null) : GameEventBase;

file record IdeoChangedContextFrame(IdeoChangeReason Reason, Pawn Converter = null);

file static class IdeoChangedContext
{
    private static readonly IdeoChangedContextFrame DefaultFrame = new(IdeoChangeReason.Unknown);
    
    public static IdeoChangedContextFrame Frame = DefaultFrame;
    public static void Clear() => Frame = DefaultFrame;
}

[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.SetIdeo))]
internal static class Pawn_IdeoTracker_SetIdeo_IdeoChanged_Patch
{
    private static void Prefix(Pawn_IdeoTracker __instance, out Ideo __state) => __state = __instance.Ideo;

    private static void Postfix(Pawn_IdeoTracker __instance, Ideo __state)
    {
        var newIdeo = __instance.Ideo;
        if (__state == newIdeo)
            return;

        var pawn = Accessor.Pawn_IdeoTracker.Pawn(__instance);
        var context = IdeoChangedContext.Frame;
        GameEventBus.Publish(new IdeoChangedEvent(pawn, __state, newIdeo, context.Reason, context.Converter));
    }
}

[HarmonyPatch(typeof(CompAbilityEffect_Convert), nameof(CompAbilityEffect_Convert.Apply))]
internal static class CompAbilityEffect_Convert_Apply_IdeoChanged_Patch
{
    private static void Prefix(CompAbilityEffect_Convert __instance)
    {
        IdeoChangedContext.Frame = new IdeoChangedContextFrame(IdeoChangeReason.ConvertAbility, __instance.parent.pawn);
    }

    private static void Finalizer() => IdeoChangedContext.Clear();
}

[HarmonyPatch(typeof(InteractionWorker_ConvertIdeoAttempt), nameof(InteractionWorker_ConvertIdeoAttempt.Interacted))]
internal static class InteractionWorker_ConvertIdeoAttempt_Interacted_IdeoChanged_Patch
{
    private static void Prefix(Pawn initiator)
    {
        IdeoChangedContext.Frame = new IdeoChangedContextFrame(IdeoChangeReason.SocialInteraction, initiator);
    }

    private static void Finalizer() => IdeoChangedContext.Clear();
}

[HarmonyPatch(typeof(RitualOutcomeEffectWorker_Conversion), nameof(RitualOutcomeEffectWorker_Conversion.Apply))]
internal static class RitualOutcomeEffectWorker_Conversion_Apply_IdeoChanged_Patch
{
    private static void Prefix(LordJob_Ritual jobRitual)
    {
        IdeoChangedContext.Frame = new IdeoChangedContextFrame(IdeoChangeReason.ConversionRitual, jobRitual.PawnWithRole("moralist"));
    }

    private static void Finalizer() => IdeoChangedContext.Clear();
}

[HarmonyPatch(typeof(RitualOutcomeEffectWorker_Speech), nameof(RitualOutcomeEffectWorker_Speech.Apply))]
internal static class RitualOutcomeEffectWorker_Speech_Apply_IdeoChanged_Patch
{
    private static void Prefix(LordJob_Ritual jobRitual)
    {
        IdeoChangedContext.Frame = new IdeoChangedContextFrame(IdeoChangeReason.SpeechRitual, jobRitual.Organizer);
    }

    private static void Finalizer() => IdeoChangedContext.Clear();
}

[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.TryJoinIdeoFromExposures))]
internal static class Pawn_IdeoTracker_TryJoinIdeoFromExposures_Patch
{
    private static void Prefix()
    {
        IdeoChangedContext.Frame = new IdeoChangedContextFrame(IdeoChangeReason.Exposure);
    }

    private static void Finalizer() => IdeoChangedContext.Clear();
}

[HarmonyPatch(typeof(MentalState_IdeoChange), nameof(MentalState_IdeoChange.PreStart))]
internal static class MentalState_IdeoChange_PreStart_IdeoChanged_Patch
{
    private static void Prefix()
    {
        IdeoChangedContext.Frame = new IdeoChangedContextFrame(IdeoChangeReason.MentalBreak);
    }

    private static void Finalizer() => IdeoChangedContext.Clear();
}
