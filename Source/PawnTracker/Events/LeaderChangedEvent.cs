using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum LeaderChangeReason
{
    Death,
    Lost,
}

public record LeaderChangedEvent(Faction Faction, Pawn NewLeader, Pawn OldLeader, LeaderChangeReason Reason) : GameEventBase;

internal static class LeaderChangedContext
{
    public static Pawn OldLeader;
    public static bool ReceivedLetter;
}

// Call order:
// - Faction.Notify_LeaderLost() prefix
// - LetterStack.ReceiveLetter()
// - Faction.Notify_LeaderLost() postfix

// - Faction.Notify_LeaderDied() prefix
// - LetterStack.ReceiveLetter()
// - Faction.Notify_LeaderDied() postfix

[HarmonyPatch(typeof(Faction), nameof(Faction.Notify_LeaderLost))]
internal static class Faction_Notify_LeaderLost_Patch
{
    private static void Prefix(Faction __instance)
    {
        LeaderChangedContext.OldLeader = __instance.leader;
    }

    private static void Postfix(Faction __instance)
    {
        if (!LeaderChangedContext.ReceivedLetter)
            return;

        GameEventBus.Publish(new LeaderChangedEvent(__instance, __instance.leader, LeaderChangedContext.OldLeader, LeaderChangeReason.Lost));
    }

    private static void Finalizer() => LeaderChangedContext.ReceivedLetter = false;
}

[HarmonyPatch(typeof(Faction), nameof(Faction.Notify_LeaderDied))]
internal static class Faction_Notify_LeaderDied_Patch
{
    private static void Prefix(Faction __instance)
    {
        LeaderChangedContext.OldLeader = __instance.leader;
    }

    private static void Postfix(Faction __instance)
    {
        if (!LeaderChangedContext.ReceivedLetter)
            return;

        GameEventBus.Publish(new LeaderChangedEvent(__instance, __instance.leader, LeaderChangedContext.OldLeader, LeaderChangeReason.Death));
    }

    private static void Finalizer() => LeaderChangedContext.ReceivedLetter = false;
}

[HarmonyPatch(typeof(LetterStack), nameof(LetterStack.ReceiveLetter), typeof(TaggedString), typeof(TaggedString), typeof(LetterDef), typeof(LookTargets), typeof(Faction), typeof(Quest), typeof(List<ThingDef>), typeof(string), typeof(int), typeof(bool))]
internal static class LetterStack_ReceiveLetter_Patch_3
{
    private static void Postfix()
    {
        LeaderChangedContext.ReceivedLetter = true;
    }
}
