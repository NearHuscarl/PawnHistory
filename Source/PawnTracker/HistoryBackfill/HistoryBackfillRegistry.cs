using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.DebugTools;

namespace PawnHistory.Source.PawnTracker.HistoryBackfill;

internal static class HistoryBackfillRegistry
{
    private static readonly Dictionary<HistoryRecordDef, HistoryBackfillDefinition> Definitions = [];

    public static IReadOnlyCollection<HistoryRecordDef> ManagedDefs => Definitions.Keys.ToList();

    public static bool TryGetDefinition(HistoryRecordDef def, out HistoryBackfillDefinition definition) => Definitions.TryGetValue(def, out definition);

    public static void Clear() => Definitions.Clear();

    public static void Register(IEnumerable<HistoryBackfillDefinition> definitions)
    {
        if (definitions == null)
            return;

        foreach (var definition in definitions)
        {
            if (definition.Def == null)
            {
                L.Warning($"HistoryBackfillDefinition.Def is empty during registration: {DebugUtility.Format(definition)}.");
                continue;
            }

            if (!Definitions.TryAdd(definition.Def, definition))
                L.Warning($"Ignoring duplicate history backfill definition for {definition.Def.defName}.");
        }
    }
}
