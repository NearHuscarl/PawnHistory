using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class MapBuilder
{
    private readonly Map map;
    private IntVec3 center;
    private readonly List<Action> actions = [];

    public MapBuilder(IntVec3? center = null)
    {
        map = Find.CurrentMap;
        this.center = center ?? map.Center;
    }

    public static MapBuilder At(IntVec3 pos) => new(pos);
    public static MapBuilder AtMouse() => new(UI.MouseCell());

    // Copied and modified from GenStep_ScatterShrines.ScatterAt()
    public MapBuilder GenerateAncientTemple(int width, int height)
    { 
        var rect = map.Center.RectAbout(width, height);
        MovePawnsOutside(rect);
        var resolveParams = new ResolveParams();
        resolveParams.rect = rect;
        resolveParams.disableSinglePawn = true;
        resolveParams.disableHives = true;
        resolveParams.makeWarningLetter = true;
        BaseGen.globalSettings.map = map;
        BaseGen.symbolStack.Push("ancientTemple", resolveParams);
        BaseGen.Generate();

        map.fogGrid.Refog(rect);
        foreach (var cell in rect.Cells)
        {
            var room = cell.GetRoom(map);

            if (room == null || room.PsychologicallyOutdoors)
                map.fogGrid.Unfog(cell);
        }

        return this;
    }

    private void BuildRoomPhysical(CellRect rect, ThingDef wallDef = null, ThingDef stuff = null, TerrainDef floorDef = null, string tag = null)
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
                new ThingBuilder(ThingDefOf.Door)
                    .MadeOf(stuff)
                    .Map(map)
                    .At(cell)
                    .PlaceMode(ThingPlaceMode.Direct)
                    .Faction(Faction.OfPlayer)
                    .Create();
            }
            else
            {
                new ThingBuilder(wallDef)
                    .MadeOf(stuff)
                    .Map(map)
                    .At(cell)
                    .PlaceMode(ThingPlaceMode.Direct)
                    .Faction(Faction.OfPlayer)
                    .Create();
            }
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
    
    private void MovePawnsOutside(CellRect rect)
    {
        foreach (var pawn in map.mapPawns.AllPawnsSpawned.ToList())
        {
            if (!rect.Contains(pawn.Position))
                continue;

            if (!CellFinder.TryFindRandomCell(map, c => c.Standable(map) && !rect.Contains(c), out var newCell))
                continue;

            pawn.DeSpawn();
            GenSpawn.Spawn(pawn, newCell, map);
        }
    }

    /// <summary>
    /// Builds a generic room with walls and floor.
    /// </summary>
    public MapBuilder BuildRoom(CellRect rect, string tag = null, ThingDef wallDef = null, ThingDef stuff = null, TerrainDef floorDef = null)
    {
        TestManager.Scenario.LastRoomRect = rect;

        actions.Add(() =>
        {
            MovePawnsOutside(rect);
            BuildRoomPhysical(rect, wallDef, stuff, floorDef, tag);
            if (!tag.NullOrEmpty())
                TestManager.Scenario.TaggedRooms[tag] = rect;
        });
        return this;
    }

    public MapBuilder BuildRoom(int width, int height, string tag = null, ThingDef wallDef = null, ThingDef stuff = null, TerrainDef floorDef = null)
    {
        return BuildRoom(CellRect.CenteredOn(map.Center, width, height), tag, wallDef, stuff, floorDef);
    }

    public MapBuilder AsBarrack(List<Pawn> assignedPawns)
    {
        AsBarrack(assignedPawns.Count);
        
        actions.Add(() =>
        {
            foreach (var pawn in assignedPawns)
            {
                var bed = RestUtility.FindBedFor(pawn);
                pawn.ownership.ClaimBedIfNonMedical(bed);
            }
        });

        return this;
    }

    public MapBuilder AsBarrack(int bedCount = 3)
    {
        actions.Add(() =>
        {
            var rect = TestManager.Scenario.LastRoomRect;
            for (var i = 0; i < bedCount; i++)
            {
                var bed = new ThingBuilder(ThingDefOf.Bed, ThingDefOf.Steel).Map(map).At(rect.CenterCell).CreateSingle<Building_Bed>();
                bed.SetFaction(Faction.OfPlayer);
            }
        });
        return this;
    }

    public MapBuilder AsHospital(int bedCount, List<Building_Bed> beds = null)
    {
        actions.Add(() =>
        {
            var rect = TestManager.Scenario.LastRoomRect;
            for (var i = 0; i < bedCount; i++)
            {
                var bed = new ThingBuilder(ThingDefOf.Bed, ThingDefOf.Steel).Map(map).At(rect.CenterCell).CreateSingle<Building_Bed>();
                bed.SetFaction(Faction.OfPlayer);
                bed.Medical = true;
                beds?.Add(bed);
            }
        });
        return WithThing(ThingDefOf.MedicineUltratech, 30);
    }

    public MapBuilder AsPrison(int prisonerCount, int bedCount = 1, List<Pawn> prisoners = null)
    {
        actions.Add(() =>
        {
            if (bedCount == 0) bedCount = prisonerCount;
            var rect = TestManager.Scenario.LastRoomRect;

            for (var i = 0; i < bedCount; i++)
            {
                var bed = new ThingBuilder(ThingDefOf.Bed, ThingDefOf.Steel).Map(map).At(rect.CenterCell).CreateSingle<Building_Bed>();
                bed.SetFaction(Faction.OfPlayer);
                bed.ForPrisoners = true;
            }
            var pawns = new PawnBuilder(prisonerCount).AsPrisoner().Position(rect.CenterCell, 1).Execute();
            prisoners?.AddRange(pawns);
        });
        return this;
    }

    public MapBuilder AsThroneRoom(Pawn owner)
    {
        actions.Add(() =>
        {
            var rect = TestManager.Scenario.LastRoomRect.ContractedBy(1);
            var throne = new ThingBuilder(ThingDefOf.Throne, ThingDefOf.Gold)
                .At(new IntVec3(rect.CenterCell.x, 0, rect.maxZ - 1))
                .PlaceMode(ThingPlaceMode.Direct)
                .Faction(Faction.OfPlayer)
                .CreateSingle<Building_Throne>();

            owner.ownership.ClaimThrone(throne);

            new ThingBuilder(ThingDefOf.Harp).Faction(Faction.OfPlayer).Create();
            for (var i = 0; i < 2; i++)
            {
                var brazier = new ThingBuilder(ThingDefOf.Brazier, ThingDefOf.Steel).Faction(Faction.OfPlayer).CreateSingle<Building>();
                brazier.TryGetComp<CompRefuelable>().Refuel(999f);
            }
            for (var i = 0; i < 4; i++)
            {
                new ThingBuilder(ThingDefOf.Column, ThingDefOf.Steel).Faction(Faction.OfPlayer).Create();
            }
        });

        return this;
    }

    public MapBuilder WithThing(string defName, int totalCount = 10) => WithThing(DefDatabase<ThingDef>.GetNamed(defName), totalCount);

    public MapBuilder WithThing(ThingDef thingDef, int totalCount = 10)
    {
        actions.Add(() =>
        {
            var interior = TestManager.Scenario.LastRoomRect.ContractedBy(1);
            var limit = thingDef.stackLimit;
            var fullStacks = totalCount / limit;
            var remainder = totalCount % limit;

            for (var i = 0; i < fullStacks; i++)
            {
                new ThingBuilder(thingDef)
                    .Map(map)
                    .Stack(limit)
                    .At(interior.RandomCell)
                    .Create();
            }

            if (remainder > 0)
            {
                new ThingBuilder(thingDef)
                    .Map(map)
                    .Stack(remainder)
                    .At(interior.RandomCell)
                    .Create();
            }

            map.resourceCounter.UpdateResourceCounts(); // unoptimized?
        });
        return this;
    }

    /// <summary>
    /// Spawns a grave. If occupied is true, generates a pawn, kills it, and buries it.
    /// </summary>
    public MapBuilder WithCasket(ThingDef thingDef, ThingDef stuff = null, bool occupied = true)
    {
        actions.Add(() =>
        {
            var interior = TestManager.Scenario.LastRoomRect.ContractedBy(1);
            var casket = new ThingBuilder(thingDef)
                .MadeOf(stuff)
                .Map(map)
                .At(interior.CenterCell)
                .Faction(Faction.OfPlayer)
                .CreateSingle<Building_Casket>();

            if (!occupied)
                return;

            var victim = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);

            if (casket is Building_CorpseCasket)
            {
                victim.Kill(null);
                casket.TryAcceptThing(victim.Corpse);
            }
            else
                casket.TryAcceptThing(victim);
        });
        return this;
    }

    public static CellRect Beside(string tag, Rot4 side, int w, int h)
    {
        if (!TestManager.Scenario.TaggedRooms.TryGetValue(tag, out var existing))
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

    public MapBuilder CollapseRoofAndCrush(Pawn pawn)
    {
        actions.Add(() =>
        {
            Find.CurrentMap.roofGrid.SetRoof(pawn.Position, RoofDefOf.RoofRockThick);
            RoofCollapserImmediate.DropRoofInCells([pawn.Position], map, []);
        });

        return this;
    }

    public void Execute()
    {
        actions.ForEach(a => a.Invoke());
    }
}
