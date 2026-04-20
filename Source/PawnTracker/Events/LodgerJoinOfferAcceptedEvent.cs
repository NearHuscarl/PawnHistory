using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record LodgerJoinOfferAcceptedEvent(Pawn Pawn, Quest Quest) : GameEventBase;

internal static class LodgerJoinOfferAcceptedContext
{
    public static void PublishIfJoinOfferAccepted(ChoiceLetter_AcceptVisitors letter)
    {
        var quest = letter.quest;
        var joinOffer = quest?.PartsListForReading
            .OfType<QuestPart_PawnJoinOffer>()
            .FirstOrDefault(part => part.outSignalPawnAccepted == letter.acceptedSignal && letter.pawns.Contains(part.pawn));

        if (joinOffer == null)
            return;

        GameEventBus.Publish(new LodgerJoinOfferAcceptedEvent(joinOffer.pawn, quest));
    }
}

[HarmonyPatch(typeof(ChoiceLetter_AcceptVisitors), "get_Option_Accept")]
internal static class ChoiceLetter_AcceptVisitors_Option_Accept_Patch
{
    private static void Postfix(ChoiceLetter_AcceptVisitors __instance, DiaOption __result)
    {
        var originalAction = __result.action;
        __result.action = () =>
        {
            LodgerJoinOfferAcceptedContext.PublishIfJoinOfferAccepted(__instance);
            originalAction();
        };
    }
}
