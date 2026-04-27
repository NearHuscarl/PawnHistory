using HarmonyLib;
using PawnHistory.Source.PawnTracker.Events;
using System.Reflection;
using Verse;

namespace PawnHistory.Source.PawnTracker;

[StaticConstructorOnStartup]
internal class PawnTracker
{
    public static readonly Harmony Harmony = new("rimworld.mod.nearhuscarl.pawnhistory");
    static PawnTracker()
    {
        Harmony.PatchAllUncategorized(Assembly.GetExecutingAssembly());

        CompHistoryManager.AttachHistoryComp();
        HediffComp_History.InjectComp();
        CompCookTracker.InjectComp();
        RecorderManager.Initialize();
    }
}
