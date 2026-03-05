using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public class HistoryRecord : IExposable
{
    /// <summary>
    /// Empty constructor is required so Scribe can instantiate it
    /// </summary>
    public HistoryRecord() {}
    public HistoryRecord(PawnEventDef eventDef, Pawn pawn, string combatLogText = null)
    {
        this.eventDef = eventDef;
        this.date = GenTicks.TicksAbs;
        this.pawn = pawn;
        this.combatLogText = combatLogText;
    }

    public PawnEventDef eventDef;
    public int date;
    public TaggedString combatLogText;
    public Pawn pawn;

    public TaggedString GetDescription()
    {
        return combatLogText == null ? eventDef.description.Formatted(pawn.NameShortColored) : combatLogText;
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref eventDef, "eventDef");
        Scribe_Values.Look(ref date, "date");
        Scribe_Values.Look(ref combatLogText, "combatLogText");
        Scribe_References.Look(ref pawn, "pawn");
    }
}