using PawnHistory.Source.PawnTracker.Events;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class ConcertHeldRecorder : HistoryTaleRecorder<ConcertHeldEvent>
{
    public override void Register()
    {
        if (!ModsConfig.RoyaltyActive)
            return;

        GameEventBus.Subscribe<ConcertHeldEvent>(CreateRecord);
    }

    public override void CreateRecord(ConcertHeldEvent e)
    {
        var organizer = e.Organizer;
        if (!ShouldRecord(organizer))
            return;

        var recordDef = HistoryRecordDefOf.ConcertHeld;
        var desc = recordDef
            .Description(organizer, "Organizer")
            .IncludePawnGrammar()
            .Resolve();

        if (!ShouldRecordTale(organizer, recordDef, desc))
            return;

        AddRecord(recordDef, organizer, desc);
    }
}
