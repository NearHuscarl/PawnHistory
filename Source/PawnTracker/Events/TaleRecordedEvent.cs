using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

internal class TaleRecordedEvent(Tale tale, Pawn pawn, object[] taleParams) : GameEventBase
{
    public Tale Tale { get; } = tale;
    public Pawn Pawn { get; } = pawn;
    public object[] Params { get; } = taleParams;
}

[HarmonyPatch(typeof(TaleRecorder), nameof(TaleRecorder.RecordTale))]
public static class TaleRecorder_RecordTale_Patch
{
    public static void Postfix(Tale __result, TaleDef def, params object[] args)
    {
        if (__result == null)
            return;

        var pawn = (Pawn)args[0];
        GameEventBus.Publish(new TaleRecordedEvent(__result, pawn, [.. args.Skip(1)]));
    }
}
