using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;

namespace PawnHistory.Source.PawnTracker;

[HarmonyPatch(typeof(Transition), nameof(Transition.CheckSignal))]
public static class Transition_CheckSignal_Patch
{
    // Insert OnTransitionChange() right before changing lordToil to get the correct trigger type safely
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);
        var executeMethod = AccessTools.Method(typeof(Transition), nameof(Transition.Execute));
        var hookMethod = AccessTools.Method(typeof(Transition_CheckSignal_Patch), nameof(OnTransitionChange));

        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].Calls(executeMethod))
            {
                // insert before Execute(lord)
                yield return new CodeInstruction(OpCodes.Ldarg_0); // __instance
                yield return new CodeInstruction(OpCodes.Ldloc_1); // index1
                yield return new CodeInstruction(OpCodes.Ldarg_1); // lord
                yield return new CodeInstruction(OpCodes.Call, hookMethod);
            }

            yield return list[i];
        }
    }

    public static void OnTransitionChange(Transition __instance, int index1, Lord lord)
    {
        var trigger = __instance.triggers[index1];
        var currentToil = lord.CurLordToil;
        var nextToil = __instance.target;
        var pawns = lord.ownedPawns.Where(PawnTracker.ShouldTrack).ToList();

        Log.Message($"{lord.LordJob}: {lord.CurLordToil}->{__instance.target} trigger={__instance.triggers[index1].GetType().Name}");

        if (lord.LordJob is LordJob_TradeWithColony)
        {
            var trader = pawns.FirstOrDefault(p => p.trader != null);
            var traderKind = trader?.trader?.traderKind?.label ?? "trader";
            HandleCaravanEvents(__instance, lord, pawns, trigger, traderKind);
        }
    }

    enum CaravanLeftReason
    {
        Timeout,
        DangerousTemperature,
        AnomalousWeather,
        Trapped,
        TraderLost,
        PawnLost,
    }

    private static void HandleCaravanEvents(Transition transition, Lord lord, List<Pawn> pawns, Trigger trigger, string traderKind)
    {
        var faction = lord.faction;
        var reason = CaravanLeftReason.Timeout;
        var nextToil = transition.target;

        if (trigger is Trigger_PawnExperiencingDangerousTemperatures)
            reason = CaravanLeftReason.DangerousTemperature;
        else if (trigger is Trigger_PawnExperiencingAnomalousWeather)
            reason = CaravanLeftReason.AnomalousWeather;
        else if (trigger is Trigger_PawnCannotReachMapEdge)
            reason = CaravanLeftReason.Trapped;
        else if (trigger is Trigger_ImportantTraderCaravanPeopleLost)
            reason = CaravanLeftReason.TraderLost;
        else if (trigger is Trigger_PawnLost || trigger is Trigger_FractionPawnsLost)
            reason = CaravanLeftReason.PawnLost;

        if (nextToil is LordToil_ExitMapAndEscortCarriers
            || nextToil is LordToil_ExitMap
            || nextToil is LordToil_ExitMapTraderFighting)
        {
            var eventDef = PawnEventDefOf.TradeCaravanLeft;

            GameEventListener.Publish(new GroupEvent(pawns, faction, eventDef, (pawn) =>
            {
                var request = new GrammarRequest();

                request.Includes.Add(eventDef.rulePackDef);
                request.Rules.Add(new Rule_String("PAWN", pawn.NameShortColored.Resolve()));
                request.Rules.Add(new Rule_String("TRADERKIND", traderKind));
                request.Rules.Add(new Rule_String("FACTION", faction.NameColored.Resolve()));
                request.Constants.Add("reason", reason.ToString());

                if (reason != CaravanLeftReason.Timeout)
                    request.Rules.Add(new Rule_String("REASON", reason.ToString()));

                return GrammarResolver.Resolve("tradeCaravanLeft", request);
            }));
        }
    }
}

