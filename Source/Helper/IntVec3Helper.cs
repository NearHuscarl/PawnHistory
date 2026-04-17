using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.Helper;

public static class IntVec3Helper
{
    public static IntVec3 GetCenter(IEnumerable<IntVec3> cells)
    {
        var list = cells.ToList();
        if (!list.Any())
            return IntVec3.Invalid;

        float x = 0f, z = 0f;
        foreach (var c in list)
        {
            x += c.x;
            z += c.z;
        }
        return new IntVec3(
            Mathf.RoundToInt(x / list.Count),
            0,
            Mathf.RoundToInt(z / list.Count)
        );
    }
}