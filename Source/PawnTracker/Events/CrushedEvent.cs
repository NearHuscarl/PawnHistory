using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Events;

public record CrushedEvent(IEnumerable<Pawn> Pawns, Map Map, IntVec3 Position) : GameEventBase;

public class CrushedContext
{
    public static IntVec3 GetCenter(IEnumerable<IntVec3> cells)
    {
        var list = cells.ToList();
        if (!list.Any())
            return IntVec3.Invalid;

        int x = 0, z = 0;
        foreach (var c in list)
        {
            x += c.x;
            z += c.z;
        }

        return new IntVec3(x / list.Count, 0, z / list.Count);
    }
}

[HarmonyPatch(typeof(RoofCollapserImmediate), nameof(RoofCollapserImmediate.DropRoofInCells), [typeof(IntVec3), typeof(Map), typeof(List<Thing>)])]
public static class RoofCollapserImmediate_DropRoofInCells_Patch
{
    public static void Postfix(IntVec3 c, Map map, List<Thing> outCrushedThings)
    {
        if (outCrushedThings == null || !outCrushedThings.Any())
            return;

        GameEventBus.Publish(new CrushedEvent(outCrushedThings.OfType<Pawn>(), map, c));
    }
}

[HarmonyPatch(typeof(RoofCollapserImmediate), nameof(RoofCollapserImmediate.DropRoofInCells), [typeof(IEnumerable<IntVec3>), typeof(Map), typeof(List<Thing>)])]
public static class RoofCollapserImmediate_DropRoofInCells_Patch_2
{
    public static void Postfix(IEnumerable<IntVec3> cells, Map map, List<Thing> outCrushedThings)
    {
        if (outCrushedThings == null || !outCrushedThings.Any())
            return;

        GameEventBus.Publish(new CrushedEvent(outCrushedThings.OfType<Pawn>(), map, CrushedContext.GetCenter(cells)));
    }
}

[HarmonyPatch(typeof(RoofCollapserImmediate), nameof(RoofCollapserImmediate.DropRoofInCells), [typeof(List<IntVec3>), typeof(Map), typeof(List<Thing>)])]
public static class RoofCollapserImmediate_DropRoofInCells_Patch_3
{
    public static void Postfix(List<IntVec3> cells, Map map, List<Thing> outCrushedThings)
    {
        if (outCrushedThings == null || !outCrushedThings.Any())
            return;

        GameEventBus.Publish(new CrushedEvent(outCrushedThings.OfType<Pawn>(), map, CrushedContext.GetCenter(cells)));
    }
}