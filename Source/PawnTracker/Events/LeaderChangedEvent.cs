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

public class LeaderChangedEvent(Pawn newLeader, Pawn oldLeader, LeaderChangeReason reason) : GameEventBase
{
    public Pawn NewLeader { get; } = newLeader;
    public Pawn OldLeader { get; } = oldLeader;
    public LeaderChangeReason Reason { get; } = reason;
}

internal class LeaderChangedContext
{
    public static Pawn oldLeader;
    public static bool receivedLetter;
}

// Call order:
// - Faction.Notify_LeaderLost() prefix
// - LetterStack.ReceiveLetter()
// - Faction.Notify_LeaderLost() postfix

// - Faction.Notify_LeaderDied() prefix
// - LetterStack.ReceiveLetter()
// - Faction.Notify_LeaderDied() postfix

[HarmonyPatch(typeof(Faction), nameof(Faction.Notify_LeaderLost))]
public static class Faction_Notify_LeaderLost_Patch
{
    public static void Prefix(Faction __instance)
    {
        LeaderChangedContext.oldLeader = __instance.leader;
    }

    public static void Postfix(Faction __instance)
    {
        if (!LeaderChangedContext.receivedLetter)
            return;

        GameEventBus.Publish(new LeaderChangedEvent(__instance.leader, LeaderChangedContext.oldLeader, LeaderChangeReason.Lost));
    }

    static void Finalizer() => LeaderChangedContext.receivedLetter = false;
}

[HarmonyPatch(typeof(Faction), nameof(Faction.Notify_LeaderDied))]
public static class Faction_Notify_LeaderDied_Patch
{
    public static void Prefix(Faction __instance)
    {
        LeaderChangedContext.oldLeader = __instance.leader;
    }

    public static void Postfix(Faction __instance)
    {
        if (!LeaderChangedContext.receivedLetter)
            return;

        GameEventBus.Publish(new LeaderChangedEvent(__instance.leader, LeaderChangedContext.oldLeader, LeaderChangeReason.Death));
    }

    static void Finalizer() => LeaderChangedContext.receivedLetter = false;
}

[HarmonyPatch(typeof(LetterStack), nameof(LetterStack.ReceiveLetter), [typeof(TaggedString), typeof(TaggedString), typeof(LetterDef), typeof(LookTargets), typeof(Faction), typeof(Quest), typeof(List<ThingDef>), typeof(string), typeof(int), typeof(bool)])]
public static class LetterStack_ReceiveLetter_Patch_3
{
    public static void Postfix()
    {
        LeaderChangedContext.receivedLetter = true;
    }
}
