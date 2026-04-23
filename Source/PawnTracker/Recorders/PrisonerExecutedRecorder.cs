using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PrisonerExecutedRecorder : RecorderBase<PrisonerExecutedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<PrisonerExecutedEvent>(CreateRecord);
    }

    public override void CreateRecord(PrisonerExecutedEvent e)
    {
        if (!ShouldRecord(e.Victim))
            return;

        var recordDef = HistoryRecordDefOf.PrisonerExecuted;
        var desc = recordDef.Description(e.Victim, "Prisoner")
            .IncludePawnGrammar()
            .WithPlayerSettlement(e.Victim.Map.Parent)
            .AddRule("Executioner", e.Executioner, addSubsymbols: true)
            .AddConstant("guilty", e.Guilty)
            .AddConstant("route", e.ExecutionRoute)
            .Resolve();

        AddRecord(recordDef, e.Victim, desc, [e.Executioner]);
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

        Expect.ThatAll([warden, prisoner]).Eventually().ToHaveHistoryRecord("[PAWN], a prisoner of the colony, was executed after being found guilty.", HistoryRecordDefOf.PrisonerExecuted);
        Expect.That(prisoner).Eventually().ToHaveHistoryRecordOf(HistoryRecordDefOf.Death, -1);
    }

    public void TestNotGuilty(TestScenario scenario)
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

        Expect.ThatAll([warden, prisoner]).Eventually().ToHaveHistoryRecord("[PAWN], a prisoner of the colony, was executed.", HistoryRecordDefOf.PrisonerExecuted);
        Expect.That(prisoner).Eventually().ToHaveHistoryRecordOf(HistoryRecordDefOf.Death, -1);
    }

    public void TestGuiltyColonist(TestScenario scenario)
    {
        Expect.Assertions(2);
        scenario.SpeedUp();
      
        scenario.Map()
            .BuildRoom(8, 8, "prison")
            .AsPrison(0, 3)
            .Execute();

        var victim = scenario.Pawn().Colonist()
            .Do(p => HealthUtility.DamageUntilDowned(p))
            .Do(p => p.guilt.Notify_Guilty())
            .CreateSingle();
        
        var warden = scenario.Pawn()
            .Colonist()
            .Do(p => CaptureUtility.OrderArrest(p, victim))
            .CreateSingle();

        scenario.WaitUntil(() => victim.IsPrisoner, () =>
        {
            victim.guest.SetExclusiveInteraction(PrisonerInteractionModeDefOf.Execution);
            scenario.Pawn(warden)
                .Colonist()
                .StartJob(JobDefOf.PrisonerExecution, victim)
                .CreateSingle();
        });

        Expect.ThatAll([warden, victim]).Eventually().ToHaveHistoryRecord("[PAWN], a guilty colonist of [PlayerSettlement], was executed after being found guilty.", HistoryRecordDefOf.PrisonerExecuted);
        Expect.That(victim).Eventually().ToHaveHistoryRecordOf(HistoryRecordDefOf.Death, -1);
    }
}
