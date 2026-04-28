using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record WeaponBondedEvent(Pawn Pawn, ThingWithComps Weapon) : GameEventBase;

internal record WeaponBondedState(bool PendingLink);

[HarmonyPatch(typeof(CompBladelinkWeapon), nameof(CompBladelinkWeapon.CodeFor))]
internal static class CompBladelinkWeapon_CodeFor_Patch
{
    private static void Prefix(CompBladelinkWeapon __instance, out WeaponBondedState __state, Pawn pawn)
    {
        __state = new WeaponBondedState(__instance.CodedPawn == null);
    }

    private static void Postfix(CompBladelinkWeapon __instance, WeaponBondedState __state, Pawn pawn)
    {
        if (__instance.CodedPawn != pawn)
            return;
        if (!__state.PendingLink)
            return;

        GameEventBus.Publish(new WeaponBondedEvent(pawn, __instance.parent));
    }
}
