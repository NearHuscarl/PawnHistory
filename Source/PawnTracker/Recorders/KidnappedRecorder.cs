using System.Linq;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class KidnappedRecorder : RecorderBase<KidnappedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<KidnappedEvent>(CreateRecord);
    }

    public override void CreateRecord(KidnappedEvent e)
    {
        if (ShouldRecord(e.Kidnapper))
        {
            var recordDef = HistoryRecordDefOf.Kidnap;
            var desc = recordDef.Description(e.Kidnapper, "Kidnapper")
                .AddRule("Victim", e.Victim)
                .AddRule("Faction", e.KidnapFaction)
                .Resolve();

            AddRecord(recordDef, e.Kidnapper, desc, [e.Victim]);
        }

        if (ShouldRecord(e.Victim))
        {
            var recordDef = HistoryRecordDefOf.Kidnapped;
            var desc = recordDef.Description(e.Victim, "Victim")
                .AddRule("Kidnapper", e.Kidnapper)
                .AddRule("Faction", e.KidnapFaction)
                .AddConstant("hasKidnapper", e.Kidnapper != null)
                .Resolve();

            AddRecord(recordDef, e.Victim, desc, [e.Kidnapper]);
        }
    }

    public void Test(TestScenario scenario)
    {
        var victim = scenario.Pawn().Colonist().CreateSingle();
        var enemy = scenario.Pawn().Enemy().CreateSingle();
        Faction.OfPirates.kidnapped.Kidnap(victim, enemy);

        Expect.That(enemy).ToHaveHistoryRecord("[Kidnapper] kidnapped [Victim] for [Faction].", HistoryRecordDefOf.Kidnap);
        Expect.That(victim).ToHaveHistoryRecord("[Victim] was kidnapped by [Kidnapper] from [Faction].", HistoryRecordDefOf.Kidnapped);
    }

    // TODO: fix and test tileid because map deinit remove that information
    public void TestMapDeinit(TestScenario scenario)
    {
        Expect.Assertions(1);

        var pawn = scenario.Pawn().Colonist().CreateSingle();
        var settlement = Find.WorldObjects.Settlements.FirstOrDefault();
        
        scenario.Caravan([pawn])
            .Attack(settlement)
            .OnMapGenerated(e =>
            {
                Current.Game.DeinitAndRemoveMap(e.Map, true);
                Expect.That(pawn).ToHaveHistoryRecord("[Victim] was kidnapped by [Faction].", HistoryRecordDefOf.Kidnapped);
            })
            .Execute();
    }
}
