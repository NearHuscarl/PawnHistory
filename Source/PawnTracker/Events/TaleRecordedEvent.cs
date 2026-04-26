using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record TaleRecordedEvent(TaleDef Tale, Pawn Pawn, object[] Params) : GameEventBase;

[HarmonyPatch(typeof(TaleRecorder), nameof(TaleRecorder.RecordTale))]
internal static class TaleRecorder_RecordTale_Patch
{
    public static void Prefix(TaleDef def, params object[] args)
    {
        if (args.Length == 0 || args[0] is not Pawn)
            return;

        var pawn = (Pawn)args[0];
        GameEventBus.Publish(new TaleRecordedEvent(def, pawn, [.. args.Skip(1)]));
    }
}
