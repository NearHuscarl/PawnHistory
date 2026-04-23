using System;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PlayerTransporterArriveRecorder : RecorderBase<PlayerTransporterArriveRecorder.Input>
{
    public record Input(List<Pawn> Pawns, ArrivalAction ArrivalAction, WorldObject WorldObject, PlanetTile Tile, Transporter Transporter);

    public enum Transporter
    {
        TransportPod,
        Shuttle,
    }
    
    public enum ArrivalAction
    {
        None,
        Attack,
        Enter,
        ArriveAsGift,
        AddedToCaravan,
        FormCaravan,
        VisitSettlement,
        VisitSite,
        VisitSpace,
    }

    public override void Register()
    {
        GameEventBus.Subscribe<PlayerTransporterArriveEvent>(e =>
        {
            WorldObject worldObject = null;
            var arrivalAction = ArrivalAction.None;
            var transporter = Transporter.TransportPod;

            switch (e.ArrivalAction)
            {
                case TransportersArrivalAction_AttackSettlement aas:
                    arrivalAction = ArrivalAction.Attack;
                    worldObject = Accessor.TransportersArrivalAction_AttackSettlement.Settlement(aas);
                    break;
                case TransportersArrivalAction_LandInSpecificCell lic:
                    arrivalAction = ArrivalAction.Enter;
                    worldObject = Accessor.TransportersArrivalAction_LandInSpecificCell.MapParent(lic);
                    break;
                case TransportersArrivalAction_Trade tr: // the trade part is handled in PlayerCaravanArrivalRecorder
                    arrivalAction = ArrivalAction.VisitSettlement;
                    worldObject = Accessor.TransportersArrivalAction_Trade.Settlement(tr);
                    break;
                case TransportersArrivalAction_VisitSettlement vs:
                    arrivalAction = ArrivalAction.VisitSettlement;
                    worldObject = Accessor.TransportersArrivalAction_VisitSettlement.Settlement(vs);
                    break;
                case TransportersArrivalAction_VisitSite avs:
                    arrivalAction = ArrivalAction.VisitSite;
                    worldObject = Accessor.TransportersArrivalAction_VisitSite.Site(avs);
                    break;
                case TransportersArrivalAction_VisitSpace visitSpace: // TODO: write test for visitSpace
                    arrivalAction = ArrivalAction.VisitSpace;
                    worldObject = Accessor.TransportersArrivalAction_VisitSpace.Parent(visitSpace);
                    break;
                case TransportersArrivalAction_GiveGift gg:
                    arrivalAction = ArrivalAction.ArriveAsGift;
                    worldObject = Accessor.TransportersArrivalAction_GiveGift.Settlement(gg);
                    break;
                case TransportersArrivalAction_GiveToCaravan gtc:
                    arrivalAction = ArrivalAction.AddedToCaravan;
                    worldObject = Accessor.TransportersArrivalAction_GiveToCaravan.Caravan(gtc);
                    break;
                case TransportersArrivalAction_FormCaravan:
                    arrivalAction = ArrivalAction.FormCaravan;
                    break;
                case TransportersArrivalAction_TransportShip transportShip: // TODO: test shuttle arrival
                    worldObject = Accessor.TransportersArrivalAction_TransportShip.MapParent(transportShip);
                    transporter = Transporter.Shuttle;
                    if (worldObject is Settlement settlement && settlement.Faction != Faction.OfPlayer)
                        arrivalAction = ArrivalAction.Attack;
                    else if (worldObject?.Faction == Faction.OfPlayer)
                        arrivalAction = ArrivalAction.Enter;
                    break;
            }

            if (arrivalAction == ArrivalAction.None)
                return;

            CreateRecord(new Input(e.Pawns, arrivalAction, worldObject, worldObject?.Tile ?? e.Tile, transporter));
        });
    }

    public override void CreateRecord(Input e)
    {
        var recordDef = HistoryRecordDefOf.PlayerTransporterArrived;

        foreach (var pawn in e.Pawns.Where(ShouldRecord))
        {
            var builder = recordDef.Description(pawn)
                .AddRule("WorldObject", e.WorldObject?.ColoredLabel, addSubsymbols: true)
                .AddRule("Faction", e.WorldObject?.Faction, addSubsymbols: true)
                .WithOthers(e.Pawns)
                .AddConstant("transporter", e.Transporter)
                .AddConstant("action", e.ArrivalAction);

            if (e.ArrivalAction == ArrivalAction.Enter)
                builder.WithPlayerSettlement(e.WorldObject).AddConstant("isColony", e.WorldObject?.Faction == Faction.OfPlayer);

            AddRecord(recordDef, pawn, builder.Resolve(), tileId: e.Tile);
        }
    }

    public Action TestAttack(TestScenario scenario)
    {
        scenario.SpeedUp();
        var pawns = scenario.Pawn(3).Colonist().Execute();
        var settlement = Find.WorldObjects.Settlements.First();

        scenario.DropPod(pawns).Attack(settlement).Execute();

        Expect.ThatAll(pawns).Eventually().ToHaveHistoryRecord("[PAWN] arrived at [WorldObject] in a transport pod with 2 others to attack [Faction].", HistoryRecordDefOf.PlayerTransporterArrived);
        return () => scenario.SlowDown();
    }

    public Action TestEnter(TestScenario scenario)
    {
        scenario.SpeedUp();
        
        var pawns = scenario.Pawn(3).Colonist().Execute();
        scenario.DropPod(pawns).Enter(Find.CurrentMap.Parent).Execute();

        Expect.ThatAll(pawns).Eventually().ToHaveHistoryRecord("[PAWN] arrived at the colony in a transport pod with 2 others.", HistoryRecordDefOf.PlayerTransporterArrived);
        
        return () => scenario.SlowDown();
    }

    public Action TestEnter2(TestScenario scenario)
    {
        scenario.SpeedUp();
        
        var pawns = scenario.Pawn(3).Colonist().Execute();
        scenario.DropPod(pawns).Enter(Find.CurrentMap.Parent).Execute();
        
        var playerSettlement = Find.WorldObjects.Settlements.First(s => s.Faction == Faction.OfPlayer);
        NamePlayerSettlementDialogUtility.Named(playerSettlement, "Super Gay Viltrum");

        Expect.ThatAll(pawns).Eventually().ToHaveHistoryRecord("[PAWN] arrived at Super Gay Viltrum in a transport pod with 2 others.", HistoryRecordDefOf.PlayerTransporterArrived);
        
        return () => scenario.SlowDown();
    }

    public Action TestArriveAsGift(TestScenario scenario)
    {
        scenario.SpeedUp();
        var pawns = scenario.Pawn(2).Colonist().Execute();
        var settlement = Find.WorldObjects.Settlements.First(s => s.Faction.RelationKindWith(Faction.OfPlayer) != FactionRelationKind.Hostile);

        scenario.DropPod(pawns).ArriveAsGifts(settlement).Execute();

        Expect.ThatAll(pawns).Eventually().ToHaveHistoryRecord("[PAWN] arrived at [WorldObject] in a transport pod with 1 other as a gift to [Faction].", HistoryRecordDefOf.PlayerTransporterArrived);
        return () => scenario.SlowDown();
    }

    public Action TestTrade(TestScenario scenario)
    {
        scenario.SpeedUp();
        var pawns = scenario.Pawn(3).Colonist().Execute();
        var settlement = Find.WorldObjects.Settlements.First(s => s.Faction.RelationKindWith(Faction.OfPlayer) != FactionRelationKind.Hostile);

        scenario.DropPod(pawns).Trade(settlement).Execute();

        Expect.ThatAll(pawns).Eventually().ToHaveHistoryRecord("[PAWN] visited [WorldObject] of [Faction] in a transport pod with 2 others.", -2);
        Expect.ThatAll(pawns).Eventually().ToHaveHistoryRecord("[PAWN]'s caravan arrived at [WorldObject] with 2 others to trade with [Faction].", -1);
        
        return () => scenario.SlowDown();
    }

    public Action TestAddedToCaravan(TestScenario scenario)
    {
        scenario.SpeedUp();
        var existingCaravanPawns = scenario.Pawn().Colonist().Execute();
        var spot = Find.WorldGrid.GetNearbyTile();
        var targetCaravan = scenario.Caravan(existingCaravanPawns).Position(spot).Execute();
        var pawns = scenario.Pawn(3).Colonist().Execute();

        scenario.DropPod(pawns).GiveToCaravan(targetCaravan).Execute();

        Expect.ThatAll(pawns).Eventually().ToHaveHistoryRecord("[PAWN] was added to [WorldObject] from a transport pod with 2 others.", HistoryRecordDefOf.PlayerTransporterArrived);
        return () => scenario.SlowDown();
    }

    public Action TestFormCaravan(TestScenario scenario)
    {
        scenario.SpeedUp();
        var pawns = scenario.Pawn(3).Colonist().Execute();

        var spot = Find.WorldGrid.GetNearbyTile();
        scenario.DropPod(pawns).FormCaravan(spot).Execute();

        Expect.ThatAll(pawns).Eventually().ToHaveHistoryRecord("[PAWN] arrived from a transport pod with 2 others to form a new caravan", HistoryRecordDefOf.PlayerTransporterArrived);
        return () => scenario.SlowDown();
    }

    public Action TestVisitSettlement(TestScenario scenario)
    {
        scenario.SpeedUp();
        var pawns = scenario.Pawn(3).Colonist().Execute();
        var settlement = Find.WorldObjects.Settlements.First(s => s.Faction.RelationKindWith(Faction.OfPlayer) != FactionRelationKind.Hostile);

        scenario.DropPod(pawns).Visit(settlement).Execute();

        Expect.ThatAll(pawns).Eventually().ToHaveHistoryRecord("[PAWN] visited [WorldObject] of [Faction] in a transport pod with 2 others.", HistoryRecordDefOf.PlayerTransporterArrived);
        return () => scenario.SlowDown();
    }

    public Action TestVisitSite(TestScenario scenario)
    {
        scenario.SpeedUp();
        scenario.Quest(QuestScriptDefOf.OpportunitySite_ItemStash).Execute();
        var site = Find.WorldObjects.AllWorldObjects.OfType<Site>().FirstOrDefault();
        var pawns = scenario.Pawn(3).Colonist().Execute();

        scenario.DropPod(pawns).Visit(site).Execute();

        Expect.ThatAll(pawns).Eventually().ToHaveHistoryRecord("[PAWN] arrived at the item stash in a transport pod with 2 others.", HistoryRecordDefOf.PlayerTransporterArrived);
        return () => scenario.SlowDown();
    }

    [SkipTest]
    public Action TestShuttle(TestScenario scenario)
    {
        scenario.SpeedUp();
        
        var shuttleDef = DefDatabase<ThingDef>.GetNamed("Shuttle");
        var shuttle = scenario.Thing(shuttleDef).CreateSingle();
        var transporter = shuttle.TryGetComp<CompTransporter>();
        var shuttleComp = shuttle.TryGetComp<CompShuttle>();
        var pawns = scenario.Pawn(3)
            .Colonist()
            .Do(p => transporter.innerContainer.TryAddOrTransfer(p))
            .Execute();
        
        SendTransportShipAwayUtility.SendTransportShipAway(shuttleComp.shipParent, false, TransportShipDropMode.PawnsOnly);

        return () => scenario.SlowDown();
    }
}
