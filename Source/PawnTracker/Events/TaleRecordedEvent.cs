using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record TaleRecordedEvent(Tale Tale, Pawn Pawn, object[] Params) : GameEventBase;

[HarmonyPatch(typeof(TaleRecorder), nameof(TaleRecorder.RecordTale))]
public static class TaleRecorder_RecordTale_Patch
{
    public static void Postfix(Tale __result, TaleDef def, params object[] args)
    {
        if (__result == null)
            return;

        if (args.Length == 0 || args[0] is not Pawn)
            return;

        var pawn = (Pawn)args[0];
        GameEventBus.Publish(new TaleRecordedEvent(__result, pawn, [.. args.Skip(1)]));
    }
}
