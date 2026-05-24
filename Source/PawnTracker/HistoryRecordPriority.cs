using System.Collections.Generic;

namespace PawnHistory.Source.PawnTracker;

internal static class HistoryRecordPriority
{
    private static readonly Dictionary<HistoryRecordDef, int> PriorityByDef = CreatePriorityTable();

    private static Dictionary<HistoryRecordDef, int> CreatePriorityTable()
    {
        var priorityByDef = new Dictionary<HistoryRecordDef, int>();
        
        Add(HistoryRecordDefOf.BodyPartScarred, 500);
        Add(HistoryRecordDefOf.BodyPartDestroyed, 500);
        Add(HistoryRecordDefOf.BodyPartRemoved, 500);

        Add(HistoryRecordDefOf.BotchedSurgery, 600);
        
        Add(HistoryRecordDefOf.Crushed, 1000);
        Add(HistoryRecordDefOf.FriendlyTrapHit, 1000);
        Add(HistoryRecordDefOf.Downed, 1000);
        Add(HistoryRecordDefOf.Kill, 1005);
        Add(HistoryRecordDefOf.Death, 1010);

        Add(HistoryRecordDefOf.RelativeDeath, 1100);
        Add(HistoryRecordDefOf.BondedAnimalDeath, 1100);
        Add(HistoryRecordDefOf.TitleInherited, 1110);
        
        Add(HistoryRecordDefOf.DeathrestOrComa, 2010);
        Add(HistoryRecordDefOf.SkillLeveledDown, 2020);

        return priorityByDef;

        void Add(HistoryRecordDef def, int priority)
        {
            if (def != null)
                priorityByDef[def] = priority;
        }
    }
    
    public static bool TryGetPriority(HistoryRecordDef def, out int priority) => PriorityByDef.TryGetValue(def, out priority);
}
