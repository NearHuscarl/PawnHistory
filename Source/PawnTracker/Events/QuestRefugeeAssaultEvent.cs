using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum QuestRefugeeAssaultReason
{
    Unknown,
    Betrayal,
    Death,
    Arrested,
    SurgeryViolation,
    PsychicRitualTarget,
}

public record QuestRefugeeAssaultEvent(List<Pawn> Refugees, Quest Quest, QuestRefugeeAssaultReason Reason, Pawn Victim) : GameEventBase;

internal static class QuestRefugeeAssaultContext
{
    public static Pawn Victim;
}

// ProcessQuestSignal()
// - AssaultColony()

[HarmonyPatch(typeof(QuestPart_RefugeeInteractions), "ProcessQuestSignal")]
internal static class QuestPart_RefugeeInteractions_ProcessQuestSignal_Patch
{
    private static void Prefix(QuestPart_RefugeeInteractions __instance, Signal signal)
    {
        var assaultSignals = new HashSet<string>
        {
            __instance.inSignalDestroyed,
            __instance.inSignalArrested,
            __instance.inSignalSurgeryViolation,
            __instance.inSignalPsychicRitualTarget,
            __instance.inSignalAssaultColony,
        };
        // Note: ProcessQuestSignal calls itself so this guard is required.
        if (!assaultSignals.Contains(signal.tag))
            return;
        signal.args.TryGetArg(SignalArgsNames.Subject, out Pawn victim);
        QuestRefugeeAssaultContext.Victim = victim;
    }
}

[HarmonyPatch(typeof(QuestPart_RefugeeInteractions), "AssaultColony")]
internal static class QuestPart_RefugeeInteractions_AssaultColony_Patch
{
    private static void Postfix(QuestPart_RefugeeInteractions __instance, HistoryEventDef reason)
    {
        var refugees = __instance.pawns.ToList();
        var quest = __instance.quest;
        GameEventBus.Publish(new QuestRefugeeAssaultEvent(refugees, quest, GetReason(reason), QuestRefugeeAssaultContext.Victim));
    }

    private static QuestRefugeeAssaultReason GetReason(HistoryEventDef reason)
    {
        if (reason == null)
            return QuestRefugeeAssaultReason.Betrayal;
        if (reason == HistoryEventDefOf.QuestPawnLost)
            return QuestRefugeeAssaultReason.Death;
        if (reason == HistoryEventDefOf.QuestPawnArrested)
            return QuestRefugeeAssaultReason.Arrested;
        if (reason == HistoryEventDefOf.PerformedHarmfulSurgery)
            return QuestRefugeeAssaultReason.SurgeryViolation;
        if (reason == HistoryEventDefOf.WasPsychicRitualTarget)
            return QuestRefugeeAssaultReason.PsychicRitualTarget;

        return QuestRefugeeAssaultReason.Unknown;
    }
}
