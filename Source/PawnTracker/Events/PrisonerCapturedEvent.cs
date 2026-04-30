using System.Linq;
using HarmonyLib;
using PawnHistory.Source.Helper;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record PrisonerCapturedEvent(Pawn Prisoner, Faction CaptureFaction, Pawn Captor, Quest Quest) : GameEventBase;

[HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.CapturedBy))]
internal class Pawn_GuestTracker_CapturedBy_Patch
{
    private static void Postfix(Pawn_GuestTracker __instance, Faction by, Pawn byPawn = null)
    {
        if (__instance.GuestStatus != GuestStatus.Prisoner)
            return;

        var prisoner = Accessor.Pawn_GuestTracker.Pawn(__instance);

        // prisoner from OpportunitySite_PrisonerWillingToJoin quest
        var quest = Find.QuestManager.QuestsListForReading.LastOrDefault(q => !q.hidden && QuestHelper.IsReward(q, prisoner));
        GameEventBus.Publish(new PrisonerCapturedEvent(prisoner, by, byPawn, quest));
    }
}