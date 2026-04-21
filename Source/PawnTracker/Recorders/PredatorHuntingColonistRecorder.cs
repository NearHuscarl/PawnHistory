using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PredatorHuntingColonistRecorder : RecorderBase<PredatorHuntingColonistEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<PredatorHuntingColonistEvent>(CreateRecord);
    }

    public override void CreateRecord(PredatorHuntingColonistEvent e)
    {
        if (!ShouldRecord(e.Prey) || e.Predator == null)
            return;

        var recordDef = HistoryRecordDefOf.PredatorHuntingColonist;
        var desc = recordDef.Description(e.Prey)
            .AddRule("Predator", e.Predator, addSubsymbols: true)
            .Resolve();

        AddRecord(recordDef, e.Prey, desc, [e.Predator]);
    }

    public void Test(TestScenario scenario)
    {
        var mapCenter = Find.CurrentMap.Center;
        var prey = scenario.Pawn().Colonist().Position(mapCenter, 0).CreateSingle();
        
        scenario.Pawn()
            .Animal(DefLookup.PawnKind.Cougar)
            .StartJob(JobDefOf.PredatorHunt, prey)
            .CreateSingle();

        Expect.That(prey).Eventually().ToHaveHistoryRecord("[PAWN] was hunted by a cougar for food.", HistoryRecordDefOf.PredatorHuntingColonist);
    }
}
