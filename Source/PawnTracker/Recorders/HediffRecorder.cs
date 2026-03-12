using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class HediffRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventListener.Subscribe<HediffPostAddEvent>(e =>
        {
            var pawn = e.Pawn;
            var hediff = e.Hediff;
            var part = e.Part;
            var dinfo = e.Dinfo;

            if (!ShouldRecord(pawn))
                return;

            if (hediff.def == HediffDefOf.Anesthetic)
                HandleAnesthetizedEvent(pawn, hediff);
        });
    }

    private void HandleAnesthetizedEvent(Pawn pawn, Hediff hediff)
    {
        var desc = PawnEventDefOf.Anesthetized.description.Formatted(
            pawn.NameShortColored.Named("PAWN"),
            hediff.LabelBase.Colorize(hediff.LabelColor).Named("ANESTHETIC")
        ).Resolve();

        AddRecord(new HistoryRecord(PawnEventDefOf.Anesthetized, pawn, desc));
    }
}
