using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class TestScenario
{
    public static CellRect LastRoomRect { get; internal set; }
    public static Dictionary<string, CellRect> TaggedRooms { get; internal set; } = [];
    public static HashSet<Pawn> ProcessedPawns { get; internal set; } = [];

    public static void ClearAll()
    {
        TaggedRooms.Clear();
        ProcessedPawns.Clear();
    }

    public PawnBuilder Pawn(int count = 1) => new(count);
    public PawnBuilder Pawn(IEnumerable<Pawn> pawns) => new PawnBuilder().WithPawns(pawns);
    public GatheringBuilder Incident(GatheringDef def) => new(def);
    public IncidentBuilder Incident(IncidentDef def) => new(def);
    public IncidentBuilder Incident(string defName) => new(DefDatabase<IncidentDef>.GetNamed(defName));
    public MapBuilder Thing(IntVec3? pos = null) => new(pos);

    public IncidentBuilder Siege()
    {
        var siegeStrategy = DefDatabase<RaidStrategyDef>.GetNamed("Siege");
        return Incident(IncidentDefOf.RaidEnemy)
            .RaidStrategy(siegeStrategy);
    }

    public IncidentBuilder RaidFriendly()
    {
        return Incident(IncidentDefOf.RaidFriendly)
            .Faction(Find.FactionManager.AllFactions.FirstOrDefault(f => f.PlayerRelationKind == FactionRelationKind.Neutral && !f.def.hidden));
    }

    internal void OpenHistoryRecordTab(Pawn pawn)
    {
        CameraJumper.TryJumpAndSelect(pawn);

        var inspectWindow = (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
        var historyTab = pawn.GetInspectTabs()?.FirstOrDefault(t => t is ITab_Pawn_History);

        if (historyTab != null)
            inspectWindow.OpenTabType = historyTab.GetType();
    }
}
