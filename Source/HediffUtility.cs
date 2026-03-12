using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnHistory.Source;

internal class HediffUtility
{
    // For each vital tag on this part, check if removing it would leave
    // zero functioning parts with that same tag
    // TODO: check vital property and remove hardcoded collection
    private static readonly HashSet<BodyPartTagDef> vitalTags =
    [
        BodyPartTagDefOf.ConsciousnessSource,
        BodyPartTagDefOf.BloodPumpingSource,
        BodyPartTagDefOf.BloodFiltrationSource,
        BodyPartTagDefOf.BloodFiltrationLiver,
        BodyPartTagDefOf.BloodFiltrationKidney,
        BodyPartTagDefOf.BreathingSource,
        BodyPartTagDefOf.BreathingPathway,
        BodyPartTagDefOf.MetabolismSource,
    ];

    public static bool IsPartVital(BodyPartRecord part, Pawn pawn)
    {
        if (part.def.tags == null || part.def.tags.Count == 0)
            return part.IsCorePart;

        foreach (var tag in part.def.tags)
        {
            if (!vitalTags.Contains(tag)) continue;

            int remainingVitalParts = pawn.health.hediffSet.GetNotMissingParts()
                .Count(p => p != part && p.def.tags != null && p.def.tags.Contains(tag));

            if (remainingVitalParts == 0)
                return true;
        }

        // Core parts (torso/head) are always vital regardless of tags
        return part.IsCorePart;
    }
}
