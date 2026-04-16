using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class DropPodBuilder
{
    private readonly List<Pawn> pawns;
    private readonly List<Action<Caravan>> processors = [];
    private TransportersArrivalAction arrivalAction;
    private PlanetTile tile;

    public DropPodBuilder(List<Pawn> pawns)
    {
        this.pawns = pawns;
    }

    public DropPodBuilder Position(PlanetTile tile1)
    {
        this.tile = tile1;
        return this;
    }

    private DropPodBuilder ArriveAction(TransportersArrivalAction arriveAction)
    {
        this.arrivalAction = arriveAction;
        return this;
    }

    public DropPodBuilder Do(Action<Caravan> action)
    {
        processors.Add(action);
        return this;
    }

    public DropPodBuilder Visit(Site site)
    {
        return ArriveAction(new TransportersArrivalAction_VisitSite(site, PawnsArrivalModeDefOf.CenterDrop))
            .Position(site.Tile);
    }

    public DropPodBuilder Attack(Settlement settlement)
    {
        return ArriveAction(new TransportersArrivalAction_AttackSettlement(settlement, PawnsArrivalModeDefOf.CenterDrop))
            .Position(settlement.Tile);
    }

    public DropPodBuilder Enter(MapParent mapParent)
    {
        var cell = DropCellFinder.TradeDropSpot(mapParent.Map);
        return ArriveAction(new TransportersArrivalAction_LandInSpecificCell(mapParent, cell))
            .Position(mapParent.Tile);
    }

    public DropPodBuilder Visit(Settlement settlement)
    {
        return ArriveAction(new TransportersArrivalAction_VisitSettlement(settlement, "MessageTransportPodsArrived"))
            .Position(settlement.Tile);
    }

    public DropPodBuilder Trade(Settlement settlement)
    {
        return ArriveAction(new TransportersArrivalAction_Trade(settlement, "MessageTransportPodsArrived"))
            .Position(settlement.Tile)
            .Do(caravan =>
            {
                var negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
                new TradeSessionBuilder(settlement, negotiator).Sell(t => t.Faction == Faction.OfPlayer).Execute();
                Find.WindowStack.TryRemove(typeof(Dialog_Trade));
            });
    }

    public DropPodBuilder ArriveAsGifts(Settlement settlement)
    {
        return ArriveAction(new TransportersArrivalAction_GiveGift(settlement))
            .Position(settlement.Tile);
    }

    public DropPodBuilder GiveToCaravan(Caravan caravan)
    {
        return ArriveAction(new TransportersArrivalAction_GiveToCaravan(caravan))
            .Position(caravan.Tile);
    }

    public DropPodBuilder FormCaravan(PlanetTile tileInput)
    {
        return ArriveAction(new TransportersArrivalAction_FormCaravan())
            .Position(tileInput);
    }

    public void Execute(bool launch = true)
    {
        var map = Find.CurrentMap;
        var cells = FindLauncherCells(map, pawns.Count);
        var groupId = Find.UniqueIDsManager.GetNextTransporterGroupID();
        var launchables = new List<CompLaunchable>();

        for (var i = 0; i < pawns.Count; i++)
        {
            var launcher = SpawnLauncher(map, cells[i]);
            var pod = SpawnPod(map, FuelingPortUtility.GetFuelingPortCell(launcher));

            var transporter = pod.TryGetComp<CompTransporter>();
            var launchable = pod.TryGetComp<CompLaunchable>();

            transporter.groupID = groupId;

            var pawn = pawns[i];
            if (pawn.Spawned)
                pawn.DeSpawn();

            transporter.GetDirectlyHeldThings().TryAdd(pawn);
            transporter.GetDirectlyHeldThings().TryAdd(ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack));
            launchables.Add(launchable);
        }

        if (!launch)
            return;
            
        launchables[0].TryLaunch(tile, arrivalAction);
        TickDelayManager.Interval(1, TestManager.Timeout, data =>
        {
            var caravan = pawns.Select(p => p.GetCaravan()).FirstOrDefault(c => c != null);
            if (caravan == null)
                return;

            data.Cancelled = true;
            processors.ForEach(processor => processor(caravan));
        });
    }

    private static Building SpawnLauncher(Map map, IntVec3 cell)
    {
        var def = DefLookup.Thing.PodLauncher;
        var launcher = (Building)ThingMaker.MakeThing(def);
        GenSpawn.Spawn(launcher, cell, map);

        var refuelable = launcher.TryGetComp<CompRefuelable>();
        refuelable?.Refuel(refuelable.Props.fuelCapacity);

        return launcher;
    }

    private static Building SpawnPod(Map map, IntVec3 cell)
    {
        var pod = (Building)ThingMaker.MakeThing(ThingDefOf.TransportPod);
        GenSpawn.Spawn(pod, cell, map);
        return pod;
    }

    private static List<IntVec3> FindLauncherCells(Map map, int count)
    {
        var launcherDef = DefLookup.Thing.PodLauncher;
        var start = DropCellFinder.TradeDropSpot(map);

        foreach (var root in GenRadial.RadialCellsAround(start, 20f, true))
        {
            var result = new List<IntVec3>();

            for (var i = 0; i < count; i++)
            {
                var launcherCell = root + new IntVec3(i * 2, 0, 0);
                if (!CanFitLauncherAndPod(map, launcherDef, launcherCell))
                    break;

                result.Add(launcherCell);
            }

            if (result.Count == count)
                return result;
        }

        throw new Exception("Could not find enough space for pod launchers and pods.");
    }

    private static bool CanFitLauncherAndPod(Map map, ThingDef launcherDef, IntVec3 launcherCell)
    {
        var occupiedRect = GenAdj.OccupiedRect(launcherCell, Rot4.South, launcherDef.Size);
        var podCell = FuelingPortUtility.GetFuelingPortCell(launcherCell, Rot4.South);

        if (!podCell.InBounds(map) || !podCell.Standable(map))
            return false;

        foreach (var cell in occupiedRect.Cells)
        {
            if (!cell.InBounds(map) || !cell.Standable(map))
                return false;
            if (cell.GetThingList(map).Count > 0)
                return false;
        }

        return podCell.GetThingList(map).Count == 0;
    }
}
