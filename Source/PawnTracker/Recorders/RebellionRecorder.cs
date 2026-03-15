using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class RebellionRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventListener.Subscribe<PrisonBreakStartedEvent>(e =>
        {
            if (!ShouldRecord(e.Initiator)) return;

            HandlePrisonBreakEvent(e);
        });
    }

    private void HandlePrisonBreakEvent(PrisonBreakStartedEvent e)
    {
        var eventDef = PawnEventDefOf.PrisonBreak;
        var others = e.EscapingPrisoners.Where(p => p != e.Initiator).ToList();
        var concerns = e.EscapingPrisoners.Cast<Thing>();

        foreach (var pawn in e.EscapingPrisoners)
        {
            var builder = eventDef.ResolveDescription("prisonBreak", pawn)
                .AddConstantIf(pawn == e.Initiator, "initiator", "true")
                .IncludePawnGrammar(pawn == e.Initiator)
                .AddRuleIf(pawn != e.Initiator, "INITIATOR", e.Initiator);

            if (pawn == e.Initiator)
                builder.WithOthers(e.EscapingPrisoners);
            else
                builder.WithOthers(others);

            AddRecord(new HistoryRecord(eventDef, pawn, builder.Resolve(), concerns));
        }
    }
}
