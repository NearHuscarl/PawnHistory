using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test.Mocks;

[HarmonyPatch(typeof(AgeInjuryUtility), nameof(AgeInjuryUtility.RandomHediffsToGainOnBirthday), typeof(ThingDef), typeof(float), typeof(float))]
internal static class AlwaysHaveCancerOnBirthday
{
    private static void Postfix(ref IEnumerable<HediffGiver_Birthday> __result, ThingDef raceDef)
    {
        if (!TestManager.Scenario.AlwaysHaveCancerOnBirthday)
            return;
        
        var sets = raceDef.race.hediffGiverSets;
        if (sets == null)
            return;
        
        var modifiedResult = new List<HediffGiver_Birthday>();
        
        foreach (var g in sets.Select(t => t.hediffGivers).Where(givers => givers != null).SelectMany(givers => givers))
        {
            if (g is not HediffGiver_Birthday birthdayGiver)
                continue;
            if (birthdayGiver.hediff != HediffDefOf.Carcinoma)
                continue;
                
            modifiedResult.Add(birthdayGiver);
            break;
        }
        
        __result = modifiedResult;
    }
}