using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.Helper;

internal static class LookTargetsUtility
{
    public static IEnumerable<Pawn> GetPawns(this LookTargets targets)
    {
        if (targets == null) yield break;

        foreach (var target in targets.targets)
        {
            if (target is { HasThing: true, Thing: Pawn pawn })
            {
                yield return pawn;
            }
        }
    }
}
