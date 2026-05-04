using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PawnHistory.Source.Helper;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record ScarHealedEvent(Pawn Pawn, Hediff Hediff, BodyPartRecord Part, ScarHealedReason Reason) : GameEventBase;

public enum ScarHealedCause
{
    None,
    Gene,
    Drug,
}

public record ScarHealedReason(ScarHealedCause Cause, Hediff Hediff = null, Gene Gene = null);

internal static class ScarHealedContext
{
    public static ScarHealedReason Reason;
}

internal sealed record ScarHealedState(List<Hediff> Snapshot);

[HarmonyPatch(typeof(HediffComp_HealPermanentWounds), nameof(HediffComp_HealPermanentWounds.TryHealRandomPermanentWound))]
internal static class ScarHealedEvent_Patch
{
    private static void Prefix(Pawn pawn, out ScarHealedState __state)
    {
        __state = new ScarHealedState(pawn.health.hediffSet.hediffs.ToList());
    }

    private static void Postfix(Pawn pawn, string cause, ScarHealedState __state)
    {
        var hediff = __state.Snapshot.ExceptList(pawn.health.hediffSet.hediffs).FirstOrDefault();
        GameEventBus.Publish(new ScarHealedEvent(pawn, hediff, hediff?.Part, ScarHealedContext.Reason));
    }
}

[HarmonyPatch(typeof(HediffComp_HealPermanentWounds), nameof(HediffComp_HealPermanentWounds.CompPostTickInterval))]
internal static class HediffComp_HealPermanentWounds_CompPostTickInterval_Patch
{
    private static void Prefix(HediffComp_HealPermanentWounds __instance) => ScarHealedContext.Reason = new ScarHealedReason(ScarHealedCause.Drug, __instance.parent);
    private static void Postfix() => ScarHealedContext.Reason = null;
}

[HarmonyPatch(typeof(Gene_Healing), nameof(Gene_Healing.TickInterval))]
internal static class Gene_Healing_TickInterval_Patch
{
    private static void Prefix(Gene_Healing __instance) => ScarHealedContext.Reason = new ScarHealedReason(ScarHealedCause.Gene, null, __instance);
    private static void Postfix() => ScarHealedContext.Reason = null;
}
