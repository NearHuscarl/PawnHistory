using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class HediffRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<HediffPostAddEvent>(e =>
        {
            var pawn = e.Pawn;
            var hediff = e.Hediff;

            if (!ShouldRecord(pawn))
                return;

            if (hediff.def == HediffDefOf.Anesthetic)
                HandleAnesthetizedEvent(pawn, hediff);
        });
    }

    private void HandleAnesthetizedEvent(Pawn pawn, Hediff hediff)
    {
        var desc = HistoryRecordDefOf.Anesthetized.ResolveDescription(pawn)
            .AddRule("ANESTHETIC", hediff)
            .Resolve();

        AddRecord(new HistoryRecord(HistoryRecordDefOf.Anesthetized, pawn, desc));
    }
}
