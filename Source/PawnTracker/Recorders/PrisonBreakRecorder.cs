using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PrisonBreakRecorder : RecorderBase<PrisonBreakStartedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<PrisonBreakStartedEvent>(CreateRecord);
    }

    public override void CreateRecord(PrisonBreakStartedEvent e)
    {
        if (e.Reason == PrisonBreakReason.Rebellion)
            CreatePrisonBreakRecord(e);
        else
            CreateJailbreakRecord(e);
    }

    private void CreatePrisonBreakRecord(PrisonBreakStartedEvent e)
    {
        var recordDef = HistoryRecordDefOf.PrisonBreak;
        var joiners = e.EscapingPrisoners.Where(p => p != e.Initiator).ToList();
        var concerns = e.EscapingPrisoners.ToList();

        foreach (var pawn in joiners)
        {
            if (!ShouldRecord(pawn)) continue;

            var desc = recordDef.Description(pawn)
                .WithOthers(joiners)
                .AddRule("Initiator", e.Initiator)
                .AddConstant("initiator", false)
                .Resolve();

            AddRecord(recordDef, pawn, desc, concerns);
        }

        if (ShouldRecord(e.Initiator))
        {
            var desc = recordDef.Description(e.Initiator)
                .IncludePawnGrammar()
                .WithOthers(e.EscapingPrisoners)
                .AddConstant("initiator", true)
                .Resolve();

            AddRecord(recordDef, e.Initiator, desc, concerns);
        }
    }

    private void CreateJailbreakRecord(PrisonBreakStartedEvent e)
    {
        var recordDef = HistoryRecordDefOf.PrisonBreak;
        var concerns = e.EscapingPrisoners.Concat(e.Initiator).ToList();

        foreach (var pawn in e.EscapingPrisoners)
        {
            if (!ShouldRecord(pawn)) continue;

            var desc = recordDef.Description(pawn)
                .WithOthers(e.EscapingPrisoners)
                .AddRule("Reason", e.LogEntryText)
                .Resolve("jailbreaker");

            AddRecord(recordDef, pawn, desc, concerns);
        }
    }

    public void Test(TestScenario scenario) => TestWithParam(scenario, 2);

    public void TestWithParam(TestScenario scenario, int prisonerCount)
    {
        scenario.SpeedUp();
        var prisoners = new List<Pawn>();

        scenario.Map()
            .BuildRoom(8, 8, tag: "Prison")
            .AsPrison(prisonerCount, prisoners: prisoners)
            .Execute();

        var initiator = prisoners[0];
        var prisoner = prisoners[1];

        TickDelayManager.Delay(100, () => PrisonBreakUtility.StartPrisonBreak(initiator));

        Expect.That(initiator)
            .Eventually()
            .ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.PrisonBreak,
                Description = "[PAWN] started a prison break. [PAWN_pronoun] broke the locks open and tried to escape[WithOthers].",
                Concerns = [..prisoners.Except(initiator)],
            });
        Expect.That(prisoner)
            .Eventually()
            .ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.PrisonBreak,
                Description = "[PAWN][AndOthers] joined [Initiator]'s prison break and tried to escape.",
                Concerns = [..prisoners.Except(prisoner)],
            });
    }

    public void TestJailbreaker(TestScenario scenario)
    {
        scenario.SpeedUp();
        var prisoners = new List<Pawn>();

        scenario.Map()
            .BuildRoom(8, 8, tag: "Prison")
            .AsPrison(3, prisoners: prisoners)
            .Execute();

        var pawn = scenario.Pawn()
            .Position(scenario.OutsideOf("Prison"))
            .Colonist()
            .Do(p => p.StartMentalBreakWithMadeUpThought(Extra.MentalBreakDefOf.Jailbreaker))
            .CreateSingle();

        Expect.That(prisoners[0])
            .Eventually()
            .ToHaveHistoryRecord(new ExpectedHistoryRecord
            {
                Def = HistoryRecordDefOf.PrisonBreak,
                Description = "[Reason] As a result, [PAWN][AndOthers] started a prison break.",
                Concerns = [..prisoners.Except(prisoners[0]), pawn],
            });
    }
}
