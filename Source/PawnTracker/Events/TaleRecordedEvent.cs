using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record TaleRecordedEvent(TaleDef Tale, Pawn Pawn, object[] Params) : GameEventBase;

[HarmonyPatch(typeof(TaleRecorder), nameof(TaleRecorder.RecordTale))]
internal static class TaleRecorder_RecordTale_Patch
{
    public static void Prefix(TaleDef def, params object[] args)
    {
        TaleEventAdapter.Publish(def, args);
    }
}
