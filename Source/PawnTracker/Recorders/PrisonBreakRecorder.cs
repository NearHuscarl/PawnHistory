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
            RecordPrisonBreak(e);
        else
            RecordJailbreak(e);
    }

    private void RecordPrisonBreak(PrisonBreakStartedEvent e)
    {
        var recordDef = HistoryRecordDefOf.PrisonBreak;
        var joiners = e.EscapingPrisoners.Where(p => p != e.Initiator).ToList();
        var concerns = e.EscapingPrisoners.Cast<Thing>();

        foreach (var pawn in e.EscapingPrisoners)
        {
            if (!ShouldRecord(pawn)) continue;

            var builder = recordDef.Description(pawn)
                .AddConstant("initiator", pawn == e.Initiator)
                .IncludePawnGrammar(pawn == e.Initiator);

            if (pawn == e.Initiator)
                builder.WithOthers(e.EscapingPrisoners);
            else
                builder.WithOthers(joiners).AddRule("Initiator", e.Initiator);

            AddRecord(recordDef, pawn, builder.Resolve(), concerns);
        }
    }

    private void RecordJailbreak(PrisonBreakStartedEvent e)
    {
        var recordDef = HistoryRecordDefOf.PrisonBreak;
        var concerns = e.EscapingPrisoners.Concat(e.Initiator).Cast<Thing>();

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

    public void Test(TestScenario scenario, int prisonerCount)
    {
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
            .ToHaveHistoryRecord("[PAWN] started a prison break. [PAWN_pronoun] broke the locks open and tried to escape[WithOthers].");
        Expect.That(prisoner)
            .Eventually()
            .ToHaveHistoryRecord("[PAWN][AndOthers] joined [Initiator]'s prison break and tried to escape.");
    }

    public void TestJailbreaker(TestScenario scenario, int prisonerCount)
    {
        var prisoners = new List<Pawn>();

        scenario.Map()
            .BuildRoom(8, 8, tag: "Prison")
            .AsPrison(prisonerCount, prisoners: prisoners)
            .Execute();

        var jailbreakerBreak = DefDatabase<MentalBreakDef>.GetNamed("Jailbreaker");
        var pawn = scenario.Pawn()
            .WithPosition(scenario.OutsideOf("Prison"))
            .ThatMatches(ShouldRecord)
            .Do(p => p.StartMentalBreakWithMadeupThought(jailbreakerBreak))
            .CreateSingle();

        Expect.That(prisoners[0])
            .Eventually()
            .ToHaveHistoryRecord("[Reason] As a result, [PAWN][AndOthers] started a prison break.");
    }
}
