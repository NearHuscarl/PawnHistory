using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public enum MiscarryReason
{
    None,
    Starvation,
    PoorHealth,
}

public record MiscarriedEvent(Pawn Carrier, MiscarryReason Reason) : GameEventBase;

[HarmonyPatch(typeof(Hediff_Pregnant), nameof(Hediff_Pregnant.TickInterval))]
internal static class Hediff_Pregnant_TickInterval_Patch
{
    private static readonly MethodInfo MiscarryMethod = AccessTools.Method(typeof(Hediff_Pregnant), nameof(Hediff_Pregnant.Miscarry));
    private static readonly MethodInfo BeforeMiscarryMethod = AccessTools.Method(typeof(Hediff_Pregnant_TickInterval_Patch), nameof(BeforeMiscarry));

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        var result = new List<CodeInstruction>();
        var miscarryCallIndex = 0;

        foreach (var code in codes)
        {
            if (code.Calls(MiscarryMethod))
            {
                miscarryCallIndex++;

                // Vanilla order:
                // 1st Miscarry() call = starvation
                // 2nd Miscarry() call = poor health
                var isStarvation = miscarryCallIndex == 1;

                // Important: move labels from the original call to the injected hook,
                // otherwise branches targeting the call could skip the hook.
                var labels = code.labels;
                code.labels = new List<Label>();

                result.Add(new CodeInstruction(OpCodes.Ldarg_0) { labels = labels });
                result.Add(new CodeInstruction(isStarvation ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
                result.Add(new CodeInstruction(OpCodes.Call, BeforeMiscarryMethod));
            }

            result.Add(code);
        }

        if (miscarryCallIndex != 2)
            L.Error($"Expected 2 Hediff_Pregnant.Miscarry calls in TickInterval, found {miscarryCallIndex}.");

        return result;
    }

    private static void BeforeMiscarry(Hediff_Pregnant pregnancy, bool starvation)
    {
        if (pregnancy.def != HediffDefOf.PregnantHuman)
            return;

        var reason = starvation
            ? MiscarryReason.Starvation
            : MiscarryReason.PoorHealth;

        GameEventBus.Publish(new MiscarriedEvent(pregnancy.pawn, reason));
    }
}