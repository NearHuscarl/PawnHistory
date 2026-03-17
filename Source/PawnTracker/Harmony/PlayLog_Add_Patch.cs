using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

[HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
internal class PlayLog_Add_Patch
{
    static readonly AccessTools.FieldRef<PlayLogEntry_Interaction, Pawn> InitiatorRef =
        AccessTools.FieldRefAccess<PlayLogEntry_Interaction, Pawn>("initiator");
    static readonly AccessTools.FieldRef<PlayLogEntry_Interaction, Pawn> RecipientRef =
        AccessTools.FieldRefAccess<PlayLogEntry_Interaction, Pawn>("recipient");

    static void Postfix(LogEntry entry)
    {
        if (entry is not PlayLogEntry_Interaction interactionEntry) return;

        var initiator = InitiatorRef(interactionEntry);
        var recipient = RecipientRef(interactionEntry);
        if (initiator == null || recipient == null) return;

        if (initiator.InMentalState && initiator.MentalStateDef == MentalStateDefOf.SocialFighting)
        {
            GameEventBus.Publish(new SocialFightStartedEvent(interactionEntry, initiator, recipient));
        }
    }
}