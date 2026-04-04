using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class PawnTradedRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<PawnTradedEvent>(e =>
        {
            HandleSoldEvent(e);
        });
    }

    private void HandleSoldEvent(PawnTradedEvent e)
    {
        if (!ShouldRecord(e.SoldVictim))
            return;

        var recordDef = e.TradeAction == TradeAction.PlayerSells ? HistoryRecordDefOf.SoldToSlavery : HistoryRecordDefOf.BoughtFromSlavery;
        var desc = recordDef.Description(e.SoldVictim)
            .AddRule("Negotiator", e.Negotiator)
            .AddRule("Faction", e.Trader.Faction)
            .AddRule("Price", e.Price)
            .AddConstant("action", e.TradeAction)
            .Resolve();
        AddRecord(recordDef, e.SoldVictim, desc, [e.Negotiator]);
    }

    [SkipTest]
    public override void Test(TestScenario scenario)
    {
        var pawns = scenario.Incident(IncidentDefOf.TraderCaravanArrival)
            .TraderKind("Caravan_Neolithic_Slaver")
            .Point(400)
            .Execute();

        scenario.Map()
            .BuildRoom(8, 8, tag: "Prison")
            .AsPrison(2)
            .WithThing(ThingDefOf.Silver, 3000)
            .Execute();

        scenario.Pawn()
            .Colonist()
            .StartJob(JobDefOf.TradeWithPawn, pawns[0])
            .Execute();
    }
}
