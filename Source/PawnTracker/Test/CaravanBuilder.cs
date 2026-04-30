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
    private Caravan caravanResult;

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
        if (action == null)
            return this;
        
        GameEventBus.SubscribeOnce<MapGeneratedEvent>(e =>
        {
            if (e.MapParent.Tile != tile)
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
        return ArriveAction(new CaravanArrivalAction_AttackSettlement(settlement)).Position(settlement.Tile);
    }

    public CaravanBuilder Enter(MapParent mapParent)
    {
        return ArriveAction(new CaravanArrivalAction_Enter(mapParent)).Position(mapParent.Tile);
    }

    public CaravanBuilder Visit(Settlement settlement)
    {
        return ArriveAction(new CaravanArrivalAction_VisitSettlement(settlement)).Position(settlement.Tile);
    }

    public CaravanBuilder VisitEscapeShip(MapParent mapParent)
    {
        return ArriveAction(new CaravanArrivalAction_VisitEscapeShip(mapParent.GetComponent<EscapeShipComp>())).Position(mapParent.Tile);
    }

    public CaravanBuilder VisitPeaceTalks(PeaceTalks peaceTalks)
    {
        return ArriveAction(new CaravanArrivalAction_VisitPeaceTalks(peaceTalks)).Position(peaceTalks.Tile);
    }

    public CaravanBuilder VisitSite(Site site)
    {
        return ArriveAction(new CaravanArrivalAction_VisitSite(site)).Position(site.Tile);
    }

    public CaravanBuilder OfferGifts(Settlement settlement)
    {
        return ArriveAction(new CaravanArrivalAction_OfferGifts(settlement))
            .Position(settlement.Tile)
            .Do(caravan =>
            {
                var negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
                new TradeDealBuilder(negotiator).WithSettlementTrader(settlement).Gift();
                Find.WindowStack.TryRemove(typeof(Dialog_Trade));
            });
    }

    public CaravanBuilder FulfillTradeRequest(Settlement settlement)
    {
        return Position(settlement.Tile)
            .Do(caravan =>
            {
                if (!settlement.TryGetComponent(out TradeRequestComp request))
                    return;

                Accessor.TradeRequestComp.Fulfill(request, caravan);
            });
    }

    public CaravanBuilder Trade(Settlement settlement)
    {
        return ArriveAction(new CaravanArrivalAction_Trade(settlement))
            .Position(settlement.Tile)
            .Do(caravan =>
            {
                var negotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
                new TradeDealBuilder(negotiator).WithSettlementTrader(settlement).Sell();
                Find.WindowStack.TryRemove(typeof(Dialog_Trade));
            });
    }

    public CaravanBuilder Give(List<Thing> things)
    {
        return Do(caravan =>
        {
            foreach (var thing in things)
            {
                if (thing.Spawned)
                    thing.DeSpawn();
                CaravanInventoryUtility.GiveThing(caravan, thing);
            }
        });
    }

    public Caravan Execute()
    {
        var caravan = CaravanMaker.MakeCaravan(pawns, Faction.OfPlayer, tile, true);
        CaravanInventoryUtility.GiveThing(caravan, ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack));

        caravan.Tile = tile;
        
        if (arrivalAction != null)
        {
            // "(Dev: instantly)"
            caravan.pather.StopDead();
            arrivalAction.Arrived(caravan);
        }
        
        processors.ForEach(processor => processor(caravan));

        caravanResult = caravan;
        return caravan;
    }
}