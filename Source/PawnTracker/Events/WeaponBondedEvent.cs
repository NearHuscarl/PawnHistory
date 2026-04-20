using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record WeaponBondedEvent(Pawn Pawn, ThingWithComps Weapon) : GameEventBase;

internal static class WeaponBondedContext
{
    public static bool PendingLink;
}

[HarmonyPatch(typeof(CompBladelinkWeapon), nameof(CompBladelinkWeapon.CodeFor))]
internal static class CompBladelinkWeapon_CodeFor_Patch
{
    private static void Prefix(CompBladelinkWeapon __instance, Pawn pawn)
    {
        if (__instance.CodedPawn == null)
            WeaponBondedContext.PendingLink = true;
    }

    private static void Postfix(CompBladelinkWeapon __instance, Pawn pawn)
    {
        if (__instance.CodedPawn != pawn)
            return;
        if (!WeaponBondedContext.PendingLink)
            return;

        GameEventBus.Publish(new WeaponBondedEvent(pawn, __instance.parent));
    }

    private static void Finalizer() => WeaponBondedContext.PendingLink = false;
}
