using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class MapBuilder
{
    private readonly Map map;
    private IntVec3 center;
    private readonly List<Action<IntVec3>> actions = [];

    public MapBuilder(IntVec3? center = null)
    {
        map = Find.CurrentMap;
        this.center = center ?? map.Center;
    }

    public static MapBuilder At(IntVec3 pos) => new(pos);
    public static MapBuilder AtMouse() => new(UI.MouseCell());

    public void BuildRoomPhysical(CellRect rect, ThingDef wallDef = null, ThingDef stuff = null, TerrainDef floorDef = null, string tag = null)
    {
        wallDef ??= ThingDefOf.Wall;
        stuff ??= ThingDefOf.Plasteel;
        floorDef ??= TerrainDefOf.MetalTile;
        var doorPositions = new HashSet<IntVec3>()
        {
            new(rect.minX + (rect.Width / 2), 0, rect.minZ),
            new(rect.minX + (rect.Width / 2), 0, rect.maxZ),
            new(rect.minX, 0, rect.minZ + (rect.Height / 2)),
            new(rect.maxX, 0, rect.minZ + (rect.Height / 2)),
        };

        GenDebug.ClearArea(rect, map);
        foreach (var cell in rect)
            map.zoneManager.ZoneAt(cell)?.RemoveCell(cell);

        foreach (var cell in rect.EdgeCells)
        {
            if (!cell.InBounds(map)) continue;

            if (doorPositions.Contains(cell))
            {
                var door = (Building_Door)ThingMaker.MakeThing(ThingDefOf.Door, stuff);
                door.SetFaction(Faction.OfPlayer);
                GenSpawn.Spawn(door, cell, map);
            }
            else
                GenSpawn.Spawn(ThingMaker.MakeThing(wallDef, stuff), cell, map);
        }

        foreach (var cell in rect.Cells)
        {
            if (!cell.InBounds(map)) continue;

            map.terrainGrid.SetTerrain(cell, floorDef);
            map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
        }

        var interior = rect.ContractedBy(1);
        var stockpile = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);
        map.zoneManager.RegisterZone(stockpile);

        if (!tag.NullOrEmpty())
            stockpile.label = "Stockpile_" + tag;

        foreach (var cell in interior.Cells)
        {
            if (cell.InBounds(map))
                stockpile.AddCell(cell);
        }

        stockpile.GetStoreSettings().filter.SetAllowAll(null);
    }

    /// <summary>
    /// Builds a generic room with walls and floor.
    /// </summary>
    public MapBuilder BuildRoom(CellRect rect, string tag = null, ThingDef wallDef = null, ThingDef stuff = null, TerrainDef floorDef = null)
    {
        TestScenario.LastRoomRect = rect;

        actions.Add(_ =>
        {
            BuildRoomPhysical(rect, wallDef, stuff, floorDef, tag);
            if (!tag.NullOrEmpty())
                TestScenario.TaggedRooms[tag] = rect;
        });
        return this;
    }

    public MapBuilder BuildRoom(int width, int height, string tag = null, ThingDef wallDef = null, ThingDef stuff = null, TerrainDef floorDef = null)
    {
        return BuildRoom(CellRect.CenteredOn(Find.CurrentMap.Center, width, height), tag, wallDef, stuff, floorDef);
    }

    public MapBuilder AsBarrack(List<Pawn> assignedPawns)
    {
        AsBarrack(assignedPawns.Count);
        
        actions.Add(center =>
        {
            foreach (var pawn in assignedPawns)
            {
                var bed = RestUtility.FindBedFor(pawn);
                pawn.ownership.ClaimBedIfNonMedical(bed);
            }
        });

        return this;
    }

    public MapBuilder AsBarrack(int bedCount = 3, List<Pawn> assignedPawns = null)
    {
        actions.Add(center =>
        {
            var rect = TestScenario.LastRoomRect;
            var interior = rect.ContractedBy(1);
            var cells = interior.Cells.Where(c => c.x % 2 == 0).Take(bedCount).ToList();

            foreach (var cell in cells)
            {
                var bed = (Building_Bed)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Bed, ThingDefOf.Steel), cell, map);
                bed.SetFaction(Faction.OfPlayer);
            }
        });
        return this;
    }

    public MapBuilder AsHospital(int bedCount, List<Building_Bed> beds = null)
    {
        actions.Add(center =>
        {
            var rect = TestScenario.LastRoomRect;
            var interior = rect.ContractedBy(1);
            var cells = interior.Cells.Where(c => c.x % 2 == 0).Take(bedCount).ToList();

            foreach (var cell in cells)
            {
                var bed = (Building_Bed)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Bed, ThingDefOf.Steel), cell, map);
                bed.SetFaction(Faction.OfPlayer);
                bed.Medical = true;
                beds?.Add(bed);
            }
        });
        return WithThing(ThingDefOf.MedicineUltratech, 30);
    }

    public MapBuilder AsPrison(int prisonerCount, int bedCount = 0, List<Pawn> prisoners = null)
    {
        actions.Add(_ =>
        {
            if (bedCount == 0) bedCount = prisonerCount;
            var rect = TestScenario.LastRoomRect;
            var interior = rect.ContractedBy(1);
            var cells = interior.Cells.Where(c => c.x % 2 == 0 && c.z > interior.minZ).Take(bedCount);

            foreach (var cell in cells)
            {
                var bed = (Building_Bed)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Bed, ThingDefOf.Steel), cell, map);
                bed.SetFaction(Faction.OfPlayer);
                bed.ForPrisoners = true;
            }
            var pawns = new PawnBuilder(prisonerCount).HumanLike().AsPrisoner().WithPosition(rect.CenterCell).Execute();
            prisoners?.AddRange(pawns);
        });
        return this;
    }

    public MapBuilder WithThing(string defName, int totalCount = 10) => WithThing(DefDatabase<ThingDef>.GetNamed(defName), totalCount);

    public MapBuilder WithThing(ThingDef thingDef, int totalCount = 10)
    {
        actions.Add(c =>
        {
            var interior = TestScenario.LastRoomRect.ContractedBy(1);
            int limit = thingDef.stackLimit;
            int fullStacks = totalCount / limit;
            int remainder = totalCount % limit;

            for (var i = 0; i < fullStacks; i++)
            {
                var thing = ThingMaker.MakeThing(thingDef);
                thing.stackCount = limit;
                GenSpawn.Spawn(thing, interior.RandomCell, map);
            }

            if (remainder > 0)
            {
                var pos = interior.RandomCell;
                var thing = ThingMaker.MakeThing(thingDef);
                thing.stackCount = remainder;
                GenSpawn.Spawn(thing, pos, map);
            }

            Find.CurrentMap.resourceCounter.UpdateResourceCounts(); // unoptimized?
        });
        return this;
    }

    /// <summary>
    /// Spawns a grave. If occupied is true, generates a pawn, kills it, and buries it.
    /// </summary>
    public MapBuilder WithCasket(ThingDef thingDef, ThingDef stuff = null, bool occupied = true)
    {
        actions.Add(_ =>
        {
            var interior = TestScenario.LastRoomRect.ContractedBy(1);
            var pos = interior.Cells.Where(x => x.Standable(Find.CurrentMap)).RandomElement();

            GenDebug.ClearArea(CellRect.SingleCell(pos), map);

            var casket = (Building_Casket)GenSpawn.Spawn(ThingMaker.MakeThing(thingDef, stuff), pos, map);
            casket.SetFaction(Faction.OfPlayer);

            if (occupied)
            {
                var victim = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);

                if (casket is Building_CorpseCasket)
                {
                    victim.Kill(null);
                    var corpse = victim.Corpse;
                    casket.TryAcceptThing(victim.Corpse);
                }
                else
                    casket.TryAcceptThing(victim);
            }
        });
        return this;
    }

    public static CellRect Beside(string tag, Rot4 side, int w, int h)
    {
        if (!TestScenario.TaggedRooms.TryGetValue(tag, out var existing))
            return CellRect.CenteredOn(Find.CameraDriver.MapPosition, w, h);

        int minX = 0, minZ = 0;

        // We align the new room's edge exactly 2 tiles away from the existing edge.
        // 1 tile for the wall, 1 tile for the walkable gap.
        if (side == Rot4.East)
        {
            minX = existing.maxX + 2;
            minZ = existing.CenterCell.z - (h - 1) / 2;
        }
        else if (side == Rot4.West)
        {
            minX = existing.minX - 2 - (w - 1);
            minZ = existing.CenterCell.z - (h - 1) / 2;
        }
        else if (side == Rot4.North)
        {
            minX = existing.CenterCell.x - (w - 1) / 2;
            minZ = existing.maxZ + 2;
        }
        else if (side == Rot4.South)
        {
            minX = existing.CenterCell.x - (w - 1) / 2;
            minZ = existing.minZ - 2 - (h - 1);
        }

        // Use the direct constructor (minX, minZ, width, height) 
        // to bypass the "CenteredOn" bias bullshit.
        return new CellRect(minX, minZ, w, h);
    }

    public void Execute()
    {
        foreach (var action in actions)
        {
            action(center);
        }
    }
}