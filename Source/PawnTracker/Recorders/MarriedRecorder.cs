using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class MarriedRecorder : RecorderBase<MarriedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<MarriedEvent>(CreateRecord);
    }

    public override void CreateRecord(MarriedEvent e)
    {
        var recordDef = HistoryRecordDefOf.Married;
        var couple = new[] { e.FirstPawn, e.SecondPawn };
        var desc = recordDef
            .Description(e.FirstPawn, "Spouse1")
            .IncludePawnGrammar()
            .AddRule("Spouse2", e.SecondPawn, addSubsymbols: true)
            .Resolve();

        foreach (var pawn in couple)
        {
            if (!ShouldRecord(pawn))
                continue;

            var otherPawn = pawn == e.FirstPawn ? e.SecondPawn : e.FirstPawn;
            AddRecord(recordDef, pawn, desc, [otherPawn]);
        }
    }

    public void Test(TestScenario scenario)
    {
        var (couple, _) = WeddingRecorder.SetupWedding(scenario);

        MarriageCeremonyUtility.Married(couple[0], couple[1]);

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.Married,
            Description = "[Spouse1] and [Spouse2] are married.",
        };
        Expect.That(couple[0]).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [couple[1]] }));
        Expect.That(couple[1]).ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [couple[0]] }));
    }
}
