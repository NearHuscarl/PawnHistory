using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class ExecutedRecorder : RecorderBase<ExecutedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<ExecutedEvent>(CreateRecord);
    }

    public override void CreateRecord(ExecutedEvent e)
    {
        var recordDef = e.Route == ExecutionRoute.Slave
            ? HistoryRecordDefOf.SlaveExecuted
            : HistoryRecordDefOf.PrisonerExecuted;

        var desc = recordDef.Description(e.Victim)
            .IncludePawnGrammar()
            .WithPlayerSettlement(e.Victim.Map.Parent)
            .AddRule("Executioner", e.Executioner, addSubsymbols: true)
            .AddConstant("guilty", e.Guilty)
            .AddConstant("route", e.Route)
            .Resolve();

        if (ShouldRecord(e.Victim))
            AddRecord(recordDef, e.Victim, desc, [e.Executioner]);
        if (ShouldRecord(e.Executioner))
            AddRecord(recordDef, e.Executioner, desc, [e.Victim]);
    }

    public void TestGuilty(TestScenario scenario)
    {
        var prisoners = new List<Pawn>();
        scenario.Map()
            .BuildRoom(8, 8, "prison")
            .AsPrison(1, prisoners: prisoners)
            .Execute();

        var prisoner = prisoners[0];
        prisoner.guest.SetExclusiveInteraction(PrisonerInteractionModeDefOf.Execution);

        var warden = scenario.Pawn()
            .Colonist()
            .StartJob(JobDefOf.PrisonerExecution, prisoner)
            .CreateSingle();

        prisoner.guilt.Notify_Guilty();
        scenario.SpeedUp();

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.PrisonerExecuted,
            Description = "[PAWN], a prisoner of the colony, was executed after being found guilty.",
        };
        Expect.That(warden).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [prisoner] }));
        Expect.That(prisoner).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [warden] }), index: -2);
        Expect.That(prisoner).Eventually().ToHaveHistoryRecordOf(HistoryRecordDefOf.Death, -1);
    }

    public void TestPrisoner(TestScenario scenario)
    {
        var prisoners = new List<Pawn>();
        scenario.Map()
            .BuildRoom(8, 8, "prison")
            .AsPrison(1, prisoners: prisoners)
            .Execute();

        var prisoner = prisoners[0];
        prisoner.guest.SetExclusiveInteraction(PrisonerInteractionModeDefOf.Execution);

        var warden = scenario.Pawn()
            .Colonist()
            .StartJob(JobDefOf.PrisonerExecution, prisoner)
            .CreateSingle();

        scenario.SpeedUp();

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.PrisonerExecuted,
            Description = "[PAWN], a prisoner of the colony, was executed.",
        };
        Expect.That(warden).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [prisoner] }));
        Expect.That(prisoner).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [warden] }), index: -2);
        Expect.That(prisoner).Eventually().ToHaveHistoryRecordOf(HistoryRecordDefOf.Death, -1);
    }

    public void TestColonist(TestScenario scenario)
    {
        scenario.SpeedUp();

        var victim = scenario.Pawn().Colonist()
            .Do(p => HealthUtility.DamageUntilDowned(p))
            .Do(p => p.guilt.Notify_Guilty())
            .Do(p => p.guilt.awaitingExecution = true) // <ExecuteColonist>
            .CreateSingle();

        var warden = scenario.Pawn()
            .Colonist()
            .StartJob(JobDefOf.PrisonerExecution, victim)
            .CreateSingle();

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.PrisonerExecuted,
            Description = "[PAWN], a colonist of [PlayerSettlement], was executed after being found guilty.",
        };
        Expect.That(warden).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [victim] }));
        Expect.That(victim).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [warden] }), index: -2);
        Expect.That(victim).Eventually().ToHaveHistoryRecordOf(HistoryRecordDefOf.Death, -1);
    }

    [RequiresIdeology]
    public void TestSlave(TestScenario scenario)
    {
        var slave = scenario.Pawn()
            .AsSlave()
            .Do(p => p.guest.slaveInteractionMode = SlaveInteractionModeDefOf.Execute)
            .CreateSingle();
        var warden = scenario.Pawn()
            .Colonist()
            .StartJob(JobDefOf.SlaveExecution, slave)
            .CreateSingle();

        scenario.SpeedUp();

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.SlaveExecuted,
            Description = "[PAWN], a slave of the colony, was executed.",
        };
        Expect.That(warden).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [slave] }));
        Expect.That(slave).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [warden] }), index: -2);
        Expect.That(slave).Eventually().ToHaveHistoryRecordOf(HistoryRecordDefOf.Death, -1);
    }
}
