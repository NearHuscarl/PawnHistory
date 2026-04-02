using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public class SocialFightStartedEvent(PlayLogEntry_Interaction interactionEntry, Pawn initiator, Pawn recipient) : GameEventBase
{
    public PlayLogEntry_Interaction InteractionEntry { get; } = interactionEntry;
    public Pawn Initiator { get; } = initiator;
    public Pawn Recipient { get; } = recipient;
}

[HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
internal class PlayLog_Add_Patch
{
    public static readonly AccessTools.FieldRef<PlayLogEntry_Interaction, Pawn> InitiatorRef =
        AccessTools.FieldRefAccess<PlayLogEntry_Interaction, Pawn>("initiator");
    public static readonly AccessTools.FieldRef<PlayLogEntry_Interaction, Pawn> RecipientRef =
        AccessTools.FieldRefAccess<PlayLogEntry_Interaction, Pawn>("recipient");
    public static readonly AccessTools.FieldRef<PlayLogEntry_Interaction, InteractionDef> InteractionDefRef =
        AccessTools.FieldRefAccess<PlayLogEntry_Interaction, InteractionDef>("intDef");

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