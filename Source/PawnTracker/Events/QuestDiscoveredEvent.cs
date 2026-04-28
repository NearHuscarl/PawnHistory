using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum QuestDiscoveredSource
{
    Unknown,
    Book,
    Trader,
    Beggar,
    Uplink,
}

// Search "SendLetterQuestAvailable(" for new source of quest

public record QuestDiscoveredEvent(Pawn Discoverer, Quest Quest, QuestDiscoveredSource Source, Thing SourceThing = null, Pawn SourcePawn = null) : GameEventBase;

internal record QuestDiscoveredState(int QuestCount);

internal static class QuestDiscoveredContext
{
    public static Quest GetNewQuest(int questCountBefore)
    {
        var quests = Find.QuestManager.QuestsListForReading;
        if (quests == null || quests.Count <= questCountBefore)
            return null;

        return quests.Skip(questCountBefore).LastOrDefault();
    }
}

[HarmonyPatch(typeof(BookOutcomeDoer_GiveQuest), "GenerateQuest")]
internal static class BookOutcomeDoer_GiveQuest_GenerateQuest_Patch
{
    private static void Prefix(out QuestDiscoveredState __state) => __state = new QuestDiscoveredState(Find.QuestManager.QuestsListForReading.Count);

    private static void Postfix(BookOutcomeDoer_GiveQuest __instance, Pawn reader, QuestDiscoveredState __state)
    {
        var quest = QuestDiscoveredContext.GetNewQuest(__state.QuestCount);
        if (quest == null)
            return;

        GameEventBus.Publish(new QuestDiscoveredEvent(reader, quest, QuestDiscoveredSource.Book, __instance.Book));
    }
}

[HarmonyPatch(typeof(TradeUtility), nameof(TradeUtility.ReceiveQuestFromTrader))]
internal static class TradeUtility_ReceiveQuestFromTrader_Patch
{
    private static void Prefix(out QuestDiscoveredState __state) => __state = new QuestDiscoveredState(Find.QuestManager.QuestsListForReading.Count);

    private static void Postfix(Pawn trader, Pawn negotiator, QuestDiscoveredState __state)
    {
        var quest = QuestDiscoveredContext.GetNewQuest(__state.QuestCount);
        if (quest == null)
            return;

        GameEventBus.Publish(new QuestDiscoveredEvent(negotiator, quest, QuestDiscoveredSource.Trader, SourcePawn: trader));
    }
}

[HarmonyPatch(typeof(CompAncientUplink), nameof(CompAncientUplink.Notify_Hacked))]
internal static class CompAncientUplink_Notify_Hacked_Patch
{
    private static void Prefix(out QuestDiscoveredState __state) => __state = new QuestDiscoveredState(Find.QuestManager.QuestsListForReading.Count);

    private static void Postfix(CompAncientUplink __instance, Pawn hacker, QuestDiscoveredState __state)
    {
        if (hacker == null)
            return;

        var quest = QuestDiscoveredContext.GetNewQuest(__state.QuestCount);
        if (quest == null)
            return;

        GameEventBus.Publish(new QuestDiscoveredEvent(hacker, quest, QuestDiscoveredSource.Uplink, __instance.parent));
    }
}

[HarmonyPatch(typeof(QuestPart_AddGiverQuest), nameof(QuestPart_AddGiverQuest.Notify_QuestSignalReceived))]
internal static class QuestPart_AddGiverQuest_Notify_QuestSignalReceived_Patch
{
    private const string BeggarTranslationKey = "QuestDiscoveredFromBeggar";

    private static void Prefix(out QuestDiscoveredState __state) => __state = new QuestDiscoveredState(Find.QuestManager.QuestsListForReading.Count);

    private static void Postfix(QuestPart_AddGiverQuest __instance, Signal signal, QuestDiscoveredState __state)
    {
        var quest = QuestDiscoveredContext.GetNewQuest(__state.QuestCount);
        if (quest == null)
            return;

        // signal.args.TryGetArg("RECEIVER", out Pawn receiver);
        // GameEventBus.Publish(new QuestDiscoveredEvent(giver, quest, QuestDiscoveredSource.Beggar, SourcePawn: receiver));
    }
}
