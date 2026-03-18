using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class PrisonBreakRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<PrisonBreakStartedEvent>(e =>
        {
            if (!ShouldRecord(e.Initiator)) return;

            if (e.Reason == PrisonBreakReason.Rebellion)
                HandlePrisonBreakEvent(e);
            else
                HandleJailbreakEvent(e);
        });
    }

    private void HandlePrisonBreakEvent(PrisonBreakStartedEvent e)
    {
        var recordDef = HistoryRecordDefOf.PrisonBreak;
        var others = e.EscapingPrisoners.Where(p => p != e.Initiator).ToList();
        var concerns = e.EscapingPrisoners.Cast<Thing>();

        foreach (var pawn in e.EscapingPrisoners)
        {
            var builder = recordDef.ResolveDescription("prisonBreak", pawn)
                .AddConstantIf(pawn == e.Initiator, "initiator", "true")
                .IncludePawnGrammar(pawn == e.Initiator)
                .AddRuleIf(pawn != e.Initiator, "INITIATOR", e.Initiator);

            if (pawn == e.Initiator)
                builder.WithOthers(e.EscapingPrisoners);
            else
                builder.WithOthers(others);

            AddRecord(recordDef, pawn, builder.Resolve(), concerns);
        }
    }

    private void HandleJailbreakEvent(PrisonBreakStartedEvent e)
    {
        var recordDef = HistoryRecordDefOf.PrisonBreak;
        var concerns = e.EscapingPrisoners.Concat(e.Initiator).Cast<Thing>();

        foreach (var pawn in e.EscapingPrisoners)
        {
            var desc = recordDef.ResolveDescription("jailbreaker", pawn)
                .WithOthers(e.EscapingPrisoners)
                .AddRule("Reason", e.LogEntryText)
                .Resolve();

            AddRecord(recordDef, pawn, desc, concerns);
        }
    }

    public void Test(TestScenario scenario, int prisonerCount)
    {
        var prisoners = new List<Pawn>();

        scenario.Thing()
            .BuildRoom(8, 8, tag: "Prison")
            .AsPrison(prisonerCount, prisoners)
            .Execute();

        TickDelayManager.Delay(100, () =>
        {
            PrisonBreakUtility.StartPrisonBreak(prisoners[0]);
        });
    }

    public void TestJailbreaker(TestScenario scenario, int prisonerCount)
    {
        var prisoners = new List<Pawn>();

        scenario.Thing()
            .BuildRoom(8, 8, tag: "Prison")
            .AsPrison(prisonerCount, prisoners)
            .Execute();

        var jailbreakerBreak = DefDatabase<MentalBreakDef>.GetNamed("Jailbreaker");
        var pawn = scenario.Pawn()
            .WithPosition(TestScenario.TaggedRooms["Prison"].OutsideOf())
            .ThatMatches(ShouldRecord)
            .CreateSingle();

        TickDelayManager.Delay(10, () =>
        {
            scenario.StartMentalBreak(pawn, jailbreakerBreak);
        });
    }
}
