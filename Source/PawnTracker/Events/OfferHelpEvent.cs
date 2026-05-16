using System.Linq;
using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using Verse;
using Verse.AI;

namespace PawnHistory.Source.PawnTracker.Events;

public record OfferHelpEvent(Pawn Rescuer, Pawn Refugee, Quest Quest) : GameEventBase;

[HarmonyPatch(typeof(Pawn_MindState), nameof(Pawn_MindState.JoinColonyBecauseRescuedBy))]
internal static class Pawn_MindState_JoinColonyBecauseRescuedBy_Patch
{
    private static void Prefix(Pawn_MindState __instance, out bool __state)
    {
        __state = __instance.pawn.Faction == Faction.OfPlayer;
    }

    private static void Postfix(Pawn_MindState __instance, Pawn by, bool __state)
    {
        if (__state)
            return;

        var refugee = __instance.pawn;
        if (refugee.Faction != Faction.OfPlayer)
            return;

        // OfferHelp is only used in the OpportunitySite_DownedRefugee/OpportunitySite_PrisonerWillingToJoin quests
        var quest = Find.QuestManager.QuestsListForReading.LastOrDefault(q => !q.hidden && QuestHelper.IsReward(q, refugee));
        GameEventBus.Publish(new OfferHelpEvent(by, refugee, quest));
    }
}
