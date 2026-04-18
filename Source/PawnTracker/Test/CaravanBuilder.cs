using System;
using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class CaravanBuilder
{
    private readonly Map map;
    private readonly List<Pawn> pawns;
    private PlanetTile tile;
    private readonly List<Action<Caravan>> processors = [];
    private CaravanArrivalAction arrivalAction;
    private MapParent mapParent;

    public CaravanBuilder(List<Pawn> pawns)
    {
        map = Find.CurrentMap;
        tile = Find.CurrentMap.Tile;
        this.pawns = pawns;
    }

    public CaravanBuilder Position(PlanetTile tile1)
    {
        this.tile = tile1;
        return this;
    }

    public CaravanBuilder Do(Action<Caravan> action)
    {
        
        processors.Add(action);
        return this;
    }

    public CaravanBuilder OnMapGenerated(Action<MapGeneratedEvent> action)
    {
        GameEventBus.SubscribeOnce<MapGeneratedEvent>(e =>
        {
            if (e.MapParent != mapParent)
                return;
            action(e);
        });
        return this;
    }

    public CaravanBuilder Camp()
    {
        TileFinder.TryFindPassableTileWithTraversalDistance(map.Tile,3,10,out var nearbyTile);
        
        processors.Add(caravan =>
        {
            var settleCommand = SettleInEmptyTileUtility.SetupCamp(caravan) as Command_Action;
            settleCommand?.action();
        });
        return Position(nearbyTile);
    }

    private CaravanBuilder ArriveAction(CaravanArrivalAction arriveAction)
    {
        this.arrivalAction = arriveAction;
        return this;
    }

    public CaravanBuilder Attack(Settlement settlement)
    {
        mapParent = settlement;
        return ArriveAction(new CaravanArrivalAction_AttackSettlement(settlement)).Position(settlement.Tile);
    }

    public CaravanBuilder Enter(MapParent mapParent1)
    {
        mapParent = mapParent1;
        return ArriveAction(new CaravanArrivalAction_Enter(mapParent)).Position(mapParent.Tile);
    }

    public CaravanBuilder Visit(Settlement settlement)
    {
        mapParent = settlement;
        return ArriveAction(new CaravanArrivalAction_VisitSettlement(settlement)).Position(settlement.Tile);
    }

    public CaravanBuilder VisitEscapeShit(MapParent mapParent1)
    {
        mapParent = mapParent1;
        return ArriveAction(new CaravanArrivalAction_VisitEscapeShip(mapParent.GetComponent<EscapeShipComp>())).Position(mapParent.Tile);
    }

    public CaravanBuilder VisitPeaceTalks(PeaceTalks peaceTalks)
    {
        return ArriveAction(new CaravanArrivalAction_VisitPeaceTalks(peaceTalks)).Position(peaceTalks.Tile);
    }

    public CaravanBuilder VisitSite(Site site)
    {
        mapParent = site;
        return ArriveAction(new CaravanArrivalAction_VisitSite(site)).Position(site.Tile);
    }

    public CaravanBuilder OfferGifts(Settlement settlement)
    {
        return ArriveAction(new CaravanArrivalAction_OfferGifts(settlement))
            .Position(settlement.Tile)
            .Do(caravan =>
            {
                var negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
                new TradeSessionBuilder(settlement, negotiator).GifMode().Sell(t => t.Faction == Faction.OfPlayer).Execute();
                Find.WindowStack.TryRemove(typeof(Dialog_Trade));
            });
    }

    public CaravanBuilder Trade(Settlement settlement)
    {
        return ArriveAction(new CaravanArrivalAction_Trade(settlement))
            .Position(settlement.Tile)
            .Do(caravan =>
            {
                var negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
                new TradeSessionBuilder(settlement, negotiator).Sell(t => t.Faction == Faction.OfPlayer).Execute();
                Find.WindowStack.TryRemove(typeof(Dialog_Trade));
            });
    }

    public Caravan Execute()
    {
        var caravan = CaravanMaker.MakeCaravan(pawns, Faction.OfPlayer, tile, true);
        CaravanInventoryUtility.GiveThing(caravan, ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack));

        if (arrivalAction != null)
        {
            // "(Dev: instantly)"
            caravan.Tile = tile;
            caravan.pather.StopDead();
            arrivalAction.Arrived(caravan);
        }
        
        processors.ForEach(processor => processor(caravan));
        
        return caravan;
    }
}