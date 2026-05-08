using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class SlaveEmancipatedRecorder : RecorderBase<SlaveEmancipatedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<SlaveEmancipatedEvent>(CreateRecord);
    }

    public override void CreateRecord(SlaveEmancipatedEvent e)
    {
        var recordDef = HistoryRecordDefOf.SlaveEmancipated;
        var desc = recordDef.Description(e.Slave, "Slave")
            .WithPlayerSettlement(e.Slave.Map.Parent)
            .AddRule("Warden", e.Warden)
            .AddConstant("cause", e.Cause)
            .Resolve();

        if (ShouldRecord(e.Warden))
            AddRecord(recordDef, e.Warden, desc, [e.Slave]);
        if (ShouldRecord(e.Slave))
            AddRecord(recordDef, e.Slave, desc, [e.Warden]);
    }

    [RequiresIdeology]
    public void Test(TestScenario scenario)
    {
        var slave = scenario.Pawn()
            .AsSlave()
            .Do(p => p.guest.slaveInteractionMode = SlaveInteractionModeDefOf.Emancipate)
            .CreateSingle();
        var warden = scenario.Pawn()
            .Colonist()
            .StartJob(JobDefOf.SlaveEmancipation, slave)
            .CreateSingle();

        scenario.SpeedUp();

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.SlaveEmancipated,
            Description = "[Slave], a slave of the colony, was released by [Warden].",
        };
        Expect.That(warden).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [slave] }));
        Expect.That(slave).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [warden] }));
    }

    [RequiresBiotech]
    [RequiresIdeology]
    public void TestBabyToChild(TestScenario scenario)
    {
        var child = scenario.Pawn()
            .Colonist()
            .AsSlave()
            .SetAge(1)
            .ForceBirthday(10)
            .CreateSingle();

        scenario.LetterBabyToChild().PickColonist().Execute();

        Expect.That(child).ToHaveHistoryRecord(HistoryRecordDefOf.SlaveEmancipated, "[Slave], a slave of the colony, was released upon becoming a child.");
    }
}
