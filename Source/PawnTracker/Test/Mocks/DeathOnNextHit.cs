using HarmonyLib;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

#if DEBUG
[HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
internal class DeathOnNextHit
{
    static void Prefix(Thing __instance, ref DamageInfo dinfo)
    {
        if (__instance.Destroyed)
            return;

        if (__instance is not Pawn pawn)
            return;

        if (TestScenario.DeathOnNextHitPawns.Remove(pawn))
            dinfo.SetAmount(99999f);
    }
}
#endif