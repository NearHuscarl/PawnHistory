using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PrisonerEscapedRecorder : RecorderBase<PrisonerEscapedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<PrisonerEscapedEvent>(CreateRecord);
    }

    public override void CreateRecord(PrisonerEscapedEvent e)
    {
        if (!ShouldRecord(e.Prisoner))
            return;

        var recordDef = HistoryRecordDefOf.PrisonerEscaped;
        var desc = recordDef.Description(e.Prisoner)
            .Resolve();

        AddRecord(recordDef, e.Prisoner, desc, location: RecordLocation.Of(e.Prisoner));
    }

    public void Test(TestScenario scenario)
    {
        scenario.SpeedUp();

        var prisoner = scenario.Pawn().AsPrisoner().CreateSingle();
        var position = prisoner.Position;

        Expect.That(prisoner).Eventually(1000).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.PrisonerEscaped,
            Description = "[PAWN] attempted to escape captivity after finding a way off the map.",
            Position = position,
        });
    }
}
