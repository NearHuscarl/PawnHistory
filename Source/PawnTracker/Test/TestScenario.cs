using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class TestScenario
{
    public static CellRect LastRoomRect { get; internal set; }
    public static Dictionary<string, CellRect> TaggedRooms { get; internal set; } = [];

    public static void ClearAll()
    {
        TaggedRooms.Clear();
    }

    public void EveryoneOnFire()
    {
        var pawns = Find.CurrentMap.mapPawns.AllPawnsSpawned
            .Where(p => p != null && p.relations != null && p.RaceProps?.Humanlike == true)
            .ToList();

        foreach (var pawn in pawns)
            FireUtility.TryAttachFire(pawn, 1.75f, null);

        Messages.Message("Everyone is now on fire!", MessageTypeDefOf.NeutralEvent);
    }

    public PawnBuilder Pawn(int count = 1) => new(count);
    public IncidentBuilder CreateIncident(IncidentDef def) => new(def);
    public MapBuilder Thing(IntVec3? pos = null) => new(pos);

    internal void OpenHistoryRecordTab(Pawn pawn)
    {
        CameraJumper.TryJumpAndSelect(pawn);

        var inspectWindow = (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
        var historyTab = pawn.GetInspectTabs()?.FirstOrDefault(t => t is ITab_Pawn_History);

        if (historyTab != null)
            inspectWindow.OpenTabType = historyTab.GetType();
    }

    public void StartMentalBreak(Pawn pawn, MentalBreakDef def)
    {
        var randomNegativeThought = DefDatabase<ThoughtDef>.AllDefs
            .Where(t => t.stages != null && t.stages.Any(s => s != null && s.baseMoodEffect < 0) && (!t.label.NullOrEmpty() || !t.stages.First().label.NullOrEmpty()))
            .RandomElementWithFallback();
        var reason = "MentalStateReason_Mood".Translate() + "\n\n" + "FinalStraw".Translate((NamedArgument)randomNegativeThought.LabelCap);

        if (!pawn.mindState.mentalBreaker.TryDoMentalBreak(reason, def))
            Log.Warning($"[PawnHistory] Failed to force mental break {def.defName} on {pawn.LabelShort}");
    }
}
