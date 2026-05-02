using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class EnslavedRecorder : RecorderBase<EnslavedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<EnslavedEvent>(CreateRecord);
    }

    public override void CreateRecord(EnslavedEvent e)
    {
        var recordDef = HistoryRecordDefOf.Enslaved;
        var interactionLog = e.LogEntryText.Split('.').Select(p => p.Trim()).FirstOrDefault(p => !p.NullOrEmpty());
        var desc = recordDef.Description(e.Slave, "Slave")
            .IncludePawnGrammar()
            .AddRule("InteractionLog", interactionLog)
            .AddRule("Enslaver", e.Enslaver, addSubsymbols: true)
            .Resolve();

        if (ShouldRecord(e.Enslaver))
            AddRecord(recordDef, e.Enslaver, desc, [e.Slave]);
        if (ShouldRecord(e.Slave))
            AddRecord(recordDef, e.Slave, desc, [e.Enslaver]);
    }

    [RequiresIdeology]
    public void Test(TestScenario scenario)
    {
        scenario.SpeedUp();

        var prisoners = new List<Pawn>();
        scenario.Map()
            .BuildRoom(8, 8)
            .AsPrison(1, prisoners: prisoners)
            .Execute();

        var prisoner = prisoners[0];
        prisoner.guest.SetExclusiveInteraction(PrisonerInteractionModeDefOf.Enslave);
        prisoner.guest.will = 0f;

        var enslaver = scenario.Pawn()
            .Colonist()
            .StartJob(JobDefOf.PrisonerEnslave, prisoner)
            .CreateSingle();

        var expected = new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.Enslaved,
            Description = "[InteractionLog]. [Slave] accepted [His] fate and became enslaved by [Enslaver].",
        };
        Expect.That(enslaver).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [prisoner] }));
        Expect.That(prisoner).Eventually().ToHaveHistoryRecord(expected.With(new ExpectedHistoryRecord { Concerns = [enslaver] }));
    }
}
