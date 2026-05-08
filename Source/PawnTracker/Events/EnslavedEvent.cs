using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum EnslavedCause
{
    SocialInteraction,
    BabyToChild,
}

public record EnslavedEvent(Pawn Slave, Pawn Enslaver, EnslavedCause Cause, string LogEntryText = null) : GameEventBase;

// Call order:
// Pawn_InteractionsTracker.TryInteractWith()
// - InteractionWorker_EnslaveAttempt.Interacted()
//  - GenGuest.TryEnslavePrisoner() --> update GuestStatus
// - Find.PlayLog.Add(entry) --> here

[HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
internal class PlayLog_Add_Patch_8
{
    private static void Postfix(LogEntry entry)
    {
        if (entry is not PlayLogEntry_Interaction interactionEntry)
            return;

        if (Accessor.PlayLogEntry_Interaction.InteractionDef(interactionEntry) != InteractionDefOf.EnslaveAttempt)
            return;

        var initiator = Accessor.PlayLogEntry_Interaction.Initiator(interactionEntry);
        var recipient = Accessor.PlayLogEntry_Interaction.Recipient(interactionEntry);
        if (initiator == null || recipient == null)
            return;

        if (!recipient.IsSlaveOfColony)
            return;

        var logEntryText = interactionEntry.ToGameStringFromPOV(recipient);
        GameEventBus.Publish(new EnslavedEvent(recipient, initiator, EnslavedCause.SocialInteraction, logEntryText));
    }
}

[HarmonyPatch(typeof(ChoiceLetter_BabyToChild), "get_Choices")]
internal static class ChoiceLetter_BabyToChild_Choices_Patch
{
    private static void Postfix(ChoiceLetter_BabyToChild __instance, ref IEnumerable<DiaOption> __result)
    {
        var options = __result.ToList();
        var enslaveText = "Enslave".Translate().CapitalizeFirst();
        var option = options.FirstOrDefault(option => Accessor.DiaOption.Text(option) == enslaveText);

        if (option == null)
            return;
        
        var originalAction = option.action;
        option.action = () =>
        {
            originalAction();

            var pawn = Accessor.ChoiceLetter_BabyToChild.Pawn(__instance);
            if (!pawn.IsSlave)
                return;

            GameEventBus.Publish(new EnslavedEvent(pawn, null, EnslavedCause.BabyToChild));
        };

        __result = options;
    }
}
