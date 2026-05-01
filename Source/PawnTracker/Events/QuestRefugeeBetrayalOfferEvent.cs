using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record QuestRefugeeBetrayalOfferEvent(Pawn FactionOpponent, List<Pawn> Lodgers, Faction RefugeeFaction, Quest Quest, Quest BetrayalQuest) : GameEventBase;

internal record QuestRefugeeBetrayalOfferState(int QuestCount);

[HarmonyPatch(typeof(QuestPart_AddQuest_RefugeeBetrayal), nameof(QuestPart_AddQuest_RefugeeBetrayal.Notify_QuestSignalReceived))]
internal static class QuestPart_AddQuest_RefugeeBetrayal_Notify_QuestSignalReceived_Patch
{
    private static void Prefix(out QuestRefugeeBetrayalOfferState __state) => __state = new QuestRefugeeBetrayalOfferState(Find.QuestManager.QuestsListForReading.Count);

    private static void Postfix(QuestPart_AddQuest_RefugeeBetrayal __instance, QuestRefugeeBetrayalOfferState __state, Signal signal)
    {
        if (signal.tag != __instance.inSignal)
            return;

        var quest = QuestDiscoveredContext.GetNewQuest(__state.QuestCount);
        if (quest?.root != QuestScriptDefOf.RefugeeBetrayal)
            return;

        var factionOpponent = __instance.factionOpponent;
        var refugeeFaction = __instance.refugeeFaction.faction;
        var parentQuest = __instance.parent;
        var lodgers = __instance.lodgers.ToList();
        GameEventBus.Publish(new QuestRefugeeBetrayalOfferEvent(factionOpponent, lodgers, refugeeFaction, parentQuest, quest));
    }
}
