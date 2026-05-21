using HarmonyLib;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

[HarmonyPatch(typeof(NamePlayerFactionAndSettlementUtility), nameof(NamePlayerFactionAndSettlementUtility.CanNameFactionNow))]
internal static class DisableNamePlayerFactionDialog
{
    private static void Postfix(ref bool __result)
    {
        if (!TestManager.IsRunningTest)
            return;

        __result = false; // remove the Dialog_NamePlayerFaction if we forward a lot of time
    }
}
