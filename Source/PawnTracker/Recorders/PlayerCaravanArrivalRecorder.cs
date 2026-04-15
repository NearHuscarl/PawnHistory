using System;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.DebugTools;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PlayerCaravanArrivalRecorder : RecorderBase<PlayerCaravanArrivalRecorder.Input>
{
    public record Input(List<Pawn> Pawns, ArrivalAction ArrivalAction, WorldObject WorldObject);

    public enum ArrivalAction
    {
        Unknown,
        Attack,
        Enter,
        OfferGifts,
        Trade,
        VisitEscapeShip,
        VisitPeaceTalks,
        VisitSite,
        VisitSettlement,
    }
    
    public override void Register()
    {
        GameEventBus.Subscribe<PlayerCaravanArriveEvent>(e =>
        {
            WorldObject worldObject = null;
            var arrivalAction = ArrivalAction.Unknown;
            
            switch (e.ArrivalAction)
            {
                case CaravanArrivalAction_AttackSettlement aas:
                    arrivalAction = ArrivalAction.Attack;
                    worldObject = Accessor.CaravanArrivalAction_AttackSettlement.Settlement(aas);
                    break;
                case CaravanArrivalAction_Enter ae:
                    arrivalAction = ArrivalAction.Enter;
                    worldObject = Accessor.CaravanArrivalAction_Enter.MapParent(ae);
                    break;
                case CaravanArrivalAction_VisitEscapeShip aves:
                    arrivalAction = ArrivalAction.VisitEscapeShip;
                    worldObject = Accessor.CaravanArrivalAction_VisitEscapeShip.Target(aves);
                    break;
                case CaravanArrivalAction_VisitPeaceTalks pt:
                    arrivalAction = ArrivalAction.VisitPeaceTalks;
                    worldObject = Accessor.CaravanArrivalAction_VisitPeaceTalks.PeaceTalks(pt);
                    break;
                case CaravanArrivalAction_VisitSite avs:
                    arrivalAction = ArrivalAction.VisitSite;
                    worldObject = Accessor.CaravanArrivalAction_VisitSite.Site(avs);
                    break;
                case CaravanArrivalAction_VisitSettlement vs:
                    arrivalAction = ArrivalAction.VisitSettlement;
                    worldObject = Accessor.CaravanArrivalAction_VisitSettlement.Settlement(vs);
                    break;
            }

            if (arrivalAction is ArrivalAction.Unknown or ArrivalAction.OfferGifts or ArrivalAction.Trade)
                return;
            
            CreateRecord(new Input(e.Pawns, arrivalAction, worldObject));
        });
        GameEventBus.Subscribe<TradedEvent>(e =>
        {
            var caravan = e.Negotiator.GetCaravan();
            if (caravan == null)
                return;
            
            if (e.Trader is not Settlement trader)
                return;
            
            var arrivalAction = TradeSession.giftMode ? ArrivalAction.OfferGifts : ArrivalAction.Trade;
            CreateRecord(new Input(caravan.pawns.InnerListForReading, arrivalAction, trader));
        });
    }

    public override void CreateRecord(Input e)
    {
        var recordDef = HistoryRecordDefOf.PlayerCaravanArrived;

        foreach (var pawn in e.Pawns)
        {
            var builder = recordDef.Description(pawn)
                .AddRule("WorldObject", e.WorldObject.ColoredLabel, addSubsymbols: true)
                .AddRule("Faction", e.WorldObject.Faction, addSubsymbols: true)
                .WithOthers(e.Pawns)
                .AddConstant("action", e.ArrivalAction);

            if (e.ArrivalAction == ArrivalAction.VisitPeaceTalks)
                builder.AddRule("Leader", e.WorldObject.Faction.leader);
            if (e.ArrivalAction == ArrivalAction.Enter)
                builder.AddConstant("isColony", e.WorldObject.Faction == Faction.OfPlayer);
            
            AddRecord(recordDef, pawn, builder.Resolve());
        }
    }

    public void TestAttack(TestScenario scenario)
    {
        var pawns = scenario.Pawn(3).Colonist().Execute();
        var settlement = Find.WorldObjects.Settlements.First();
        scenario.Caravan(pawns).Attack(settlement).Execute();
        
        Expect.ThatAll(pawns).ToHaveHistoryRecord("[PAWN]'s caravan arrived at [WorldObject] to attack [Faction] with 2 others.", HistoryRecordDefOf.PlayerCaravanArrived);
    }

    public void TestEnter(TestScenario scenario)
    {
        var pawns = scenario.Pawn(3).Colonist().Execute();
        var settlement = Find.WorldObjects.Settlements.First(s => s.Faction == Faction.OfPlayer);
        scenario.Caravan(pawns).Enter(settlement).Execute();

        Expect.ThatAll(pawns).ToHaveHistoryRecord("[PAWN]'s caravan arrived at the colony with 2 others.", HistoryRecordDefOf.PlayerCaravanArrived);
        
        var pawns2 = scenario.Pawn(3).Colonist().Execute();
        var settlement2 = Find.WorldObjects.Settlements.First(s => s.Faction.RelationKindWith(Faction.OfPlayer) != FactionRelationKind.Hostile);
        scenario.Caravan(pawns2).Enter(settlement2).Execute();

        Expect.ThatAll(pawns2).ToHaveHistoryRecord("[PAWN]'s caravan arrived at [WorldObject] of [Faction] with 2 others.", HistoryRecordDefOf.PlayerCaravanArrived);
    }

    public void TestOfferGifts(TestScenario scenario)
    {
        var pawns = scenario.Pawn(3).Colonist().Execute();
        var settlement = Find.WorldObjects.Settlements.First(s => s.Faction.RelationKindWith(Faction.OfPlayer) != FactionRelationKind.Hostile);
        scenario.Caravan(pawns).OfferGifts(settlement).Execute();

        Expect.ThatAll(pawns).ToHaveHistoryRecord("[PAWN]'s caravan arrived at [WorldObject] with 2 others to offer gifts to [Faction].", HistoryRecordDefOf.PlayerCaravanArrived);
    }

    public void TestTrade(TestScenario scenario)
    {
        var pawns = scenario.Pawn(3).Colonist().Execute();
        var settlement = Find.WorldObjects.Settlements.First(s => s.Faction.RelationKindWith(Faction.OfPlayer) != FactionRelationKind.Hostile);
        scenario.Caravan(pawns).Trade(settlement).Execute();

        Expect.ThatAll(pawns).ToHaveHistoryRecord("[PAWN]'s caravan arrived at [WorldObject] with 2 others to trade with [Faction].", HistoryRecordDefOf.PlayerCaravanArrived);
    }

    public Action TestEscapeShip(TestScenario scenario)
    {
        NearDebugSettings.ShipEscapeSpawnNearby = true;

        var pawns = scenario.Pawn(3).Colonist().Execute();
        scenario.Incident(DefLookup.Incident.GiveQuest_EndGame_ShipEscape).Execute();
        var escapeShip =  Find.WorldObjects.AllWorldObjects.OfType<EscapeShip>().FirstOrDefault();
        scenario.Caravan(pawns).VisitEscapeShit(escapeShip).Execute();
        
        Expect.ThatAll(pawns).ToHaveHistoryRecord("[PAWN]'s caravan arrived at the starship with 2 others that can be used to escape the planet.", HistoryRecordDefOf.PlayerCaravanArrived);
        
        return () => NearDebugSettings.ShipEscapeSpawnNearby = false;
    }

    public void TestPeaceTalks(TestScenario scenario)
    {
        QuestUtility.GenerateQuestAndMakeAvailable(DefLookup.QuestScript.OpportunitySite_PeaceTalks, 500);
        var peaceTalks = Find.WorldObjects.AllWorldObjects.OfType<PeaceTalks>().FirstOrDefault();
        var pawns = scenario.Pawn(3).Colonist().Execute();

        scenario.Caravan(pawns).VisitPeaceTalks(peaceTalks).Execute();
        Expect.ThatAll(pawns).ToHaveHistoryRecord("[PAWN]'s caravan arrived for peace talks with 2 others to negotiate with [Leader], [Title] of [Faction].", HistoryRecordDefOf.PlayerCaravanArrived);
    }

    public void TestVisitSite(TestScenario scenario)
    {
        QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.OpportunitySite_ItemStash, 500);
        var site = Find.WorldObjects.AllWorldObjects.OfType<Site>().FirstOrDefault();
        var pawns = scenario.Pawn(3).Colonist().Execute();

        scenario.Caravan(pawns).VisitSite(site).Execute();
        Expect.ThatAll(pawns).ToHaveHistoryRecord("[PAWN]'s caravan arrived at the item stash with 2 others.", HistoryRecordDefOf.PlayerCaravanArrived);
    }

    public void TestVisitSettlement(TestScenario scenario)
    {
        var pawns = scenario.Pawn(3).Colonist().Execute();
        var settlement = Find.WorldObjects.Settlements.First(s => s.Faction.RelationKindWith(Faction.OfPlayer) != FactionRelationKind.Hostile);
        scenario.Caravan(pawns).Visit(settlement).Execute();

        Expect.ThatAll(pawns).ToHaveHistoryRecord("[PAWN]'s caravan visited [WorldObject] of [Faction] with 2 others.", HistoryRecordDefOf.PlayerCaravanArrived);
    }

    public void TestCamp(TestScenario scenario)
    {
        // TODO: do it
        var pawns = scenario.Pawn(3).Colonist().Execute();
        var caravan = scenario.Caravan(pawns).Camp().Execute();
    }
}