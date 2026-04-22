using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PrisonerReleasedRecorder : RecorderBase<PrisonerReleasedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<PrisonerReleasedEvent>(CreateRecord);
    }

    public override void CreateRecord(PrisonerReleasedEvent e)
    {
        if (!ShouldRecord(e.Prisoner))
            return;

        var recordDef = HistoryRecordDefOf.PrisonerReleased;
        var desc = recordDef.Description(e.Prisoner, "Prisoner")
            .IncludePawnGrammar()
            .WithPlayerSettlement(e.Releaser.Map.Parent)
            .AddRule("Releaser", e.Releaser, addSubsymbols: true)
            .Resolve();

        AddRecord(recordDef, e.Prisoner, desc, [e.Releaser]);
    }

    public void Test(TestScenario scenario)
    {
        var prisoners = new List<Pawn>();
        
        scenario.Map()
            .BuildRoom(8, 8, "prison")
            .AsPrison(1, prisoners: prisoners)
            .Execute();

        var prisoner = prisoners[0];
        prisoner.guest.SetExclusiveInteraction(PrisonerInteractionModeDefOf.Release);

        var releaser = scenario.Pawn()
            .Colonist()
            .StartJob(JobDefOf.ReleasePrisoner, prisoner, scenario.OutsideOf("prison"))
            .CreateSingle();

        scenario.SpeedUp();

        Expect.That(prisoner).Eventually().ToHaveHistoryRecord("[Prisoner], a prisoner of the colony, was released by [Releaser].", HistoryRecordDefOf.PrisonerReleased);
        Expect.That(prisoner).Eventually().ToHaveHistoryRecordConcern(releaser, HistoryRecordDefOf.PrisonerReleased);
        Expect.That(releaser).Eventually().Not().ToHaveHistoryRecordOf(HistoryRecordDefOf.PrisonerReleased);
    }
}
