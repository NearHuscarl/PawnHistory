using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record PsychicBondedEvent(Pawn Initiator, Pawn Recipient) : GameEventBase;

[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.TryCreatePsychicBondBetween))]
internal static class InteractionWorker_RomanceAttempt_TryCreatePsychicBondBetween_Patch
{
    private static void Postfix(bool __result, Pawn initiator, Pawn recipient)
    {
        if (!__result)
            return;

        GameEventBus.Publish(new PsychicBondedEvent(initiator, recipient));
    }
}
