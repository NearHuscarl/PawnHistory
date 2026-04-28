using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class RansomRecorder : RecorderBase<RansomEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<RansomEvent>(CreateRecord);
    }

    public override void CreateRecord(RansomEvent e)
    {
        if (e.Result == RansomResult.Postponed)
            return;

        if (!ShouldRecord(e.Hostage))
            return;
        
        var recordDef = HistoryRecordDefOf.Ransom;
        var desc = recordDef.Description(e.Hostage, "Hostage")
            .AddRule("SilverCount", e.SilverCount)
            .AddRule("EnemyFaction", e.EnemyFaction)
            .AddConstant("result", e.Result)
            .Resolve();

        AddRecord(recordDef, e.Hostage, desc);
    }

    public void TestReject(TestScenario scenario)
    {
        var hostage = scenario.Pawn().Colonist().CreateSingle();
        var enemy = scenario.Pawn().Enemy().CreateSingle(false);
        
        Faction.OfPirates.kidnapped.Kidnap(hostage, enemy);
        scenario.Incident(Extra.IncidentDefOf.RansomDemand).Execute();
        scenario.Letter<ChoiceLetter_RansomDemand>().Reject();

        Expect.That(hostage).ToHaveHistoryRecord(HistoryRecordDefOf.Ransom, "[EnemyFaction] demanded [n] silvers for [Hostage]'s release, but the colony refused.");
    }

    public void TestAccept(TestScenario scenario)
    {
        var hostage = scenario.Pawn().Colonist().CreateSingle();
        var enemy = scenario.Pawn().Enemy().CreateSingle(false);
        
        Faction.OfPirates.kidnapped.Kidnap(hostage, enemy);
        scenario.Incident(Extra.IncidentDefOf.RansomDemand).Execute();
        scenario.Map().BuildRoom(8, 8).AsBank().Execute();
        scenario.Letter<ChoiceLetter_RansomDemand>().Accept();

        Expect.That(hostage).ToHaveHistoryRecord(HistoryRecordDefOf.Ransom, "The colony paid [EnemyFaction] [n] silvers to ransom [Hostage].");
    }
}
