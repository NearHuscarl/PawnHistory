using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record SocialFightStartedEvent(PlayLogEntry_Interaction InteractionEntry, Pawn Initiator, Pawn Recipient) : GameEventBase;

[HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
internal class PlayLog_Add_Patch
{
    static void Postfix(LogEntry entry)
    {
        if (entry is not PlayLogEntry_Interaction interactionEntry) return;

        var initiator = Accessor.PlayLogEntry_Interaction.Initiator(interactionEntry);
        var recipient = Accessor.PlayLogEntry_Interaction.Recipient(interactionEntry);
        if (initiator == null || recipient == null) return;

        if (initiator.InMentalState && initiator.MentalStateDef == MentalStateDefOf.SocialFighting)
        {
            GameEventBus.Publish(new SocialFightStartedEvent(interactionEntry, initiator, recipient));
        }
    }
}