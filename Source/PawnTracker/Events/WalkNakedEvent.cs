using HarmonyLib;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

internal class WalkNakedEvent(Tale tale, Pawn pawn) : GameEventBase
{
    public Tale Tale { get; } = tale;
    public Pawn Pawn { get; } = pawn;
}

[HarmonyPatch(typeof(TaleRecorder), nameof(TaleRecorder.RecordTale))]
public static class TaleRecorder_RecordTale_Patch
{
    public static void Postfix(Tale __result, TaleDef def, params object[] args)
    {
        if (def != TaleDefOf.WalkedNaked)
            return;

        if (__result == null)
            return;

        var pawn = (Pawn)args[0];
        GameEventBus.Publish(new WalkNakedEvent(__result, pawn));
    }
}
