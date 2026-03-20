using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;
using static Verse.DamageWorker;

namespace PawnHistory.Source.PawnTracker.Events;

public class HediffAddEvent(Pawn pawn, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo) : GameEventBase
{
    public Pawn Pawn { get; } = pawn;
    public Hediff Hediff { get; } = hediff;
    public BodyPartRecord Part { get; } = part;
    public DamageInfo? Dinfo { get; } = dinfo;
}

public class HediffAddedEvent(Pawn pawn, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo) : GameEventBase
{
    public Pawn Pawn { get; } = pawn;
    public Hediff Hediff { get; } = hediff;
    public BodyPartRecord Part { get; } = part;
    public DamageInfo? Dinfo { get; } = dinfo;
}

[HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), [typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageResult)])]
internal class Pawn_HealthTracker_AddHediff_Patch
{
    public static readonly AccessTools.FieldRef<Pawn_HealthTracker, Pawn> PawnRef = AccessTools.FieldRefAccess<Pawn_HealthTracker, Pawn>("pawn");

    // Insert OnHediffAdd() right after `hediff.Part = part;` to get the resolved BodyPartRecord.
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = new List<CodeInstruction>(instructions);

        var addDirect = AccessTools.Method(typeof(HediffSet), "AddDirect");
        var hookMethod = AccessTools.Method(typeof(Pawn_HealthTracker_AddHediff_Patch), nameof(OnHediffAdd));

        for (var i = 0; i < list.Count; i++)
        {
            var code = list[i];

            if (code.Calls(addDirect))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                yield return new CodeInstruction(OpCodes.Ldarg_1); // hediff
                yield return new CodeInstruction(OpCodes.Ldarg_3); // dinfo
                yield return new CodeInstruction(OpCodes.Call, hookMethod);
            }

            yield return code;
        }
    }

    static void OnHediffAdd(Pawn_HealthTracker __instance, Hediff hediff, DamageInfo? dinfo)
    {
        var pawn = PawnRef(__instance);
        var part = hediff.Part;
        GameEventBus.Publish(new HediffAddEvent(pawn, hediff, part, dinfo));
    }

    static void Postfix(Pawn_HealthTracker __instance, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageResult result)
    {
        var pawn = PawnRef(__instance);
        GameEventBus.Publish(new HediffAddedEvent(pawn, hediff, hediff.Part, dinfo));
    }
}
