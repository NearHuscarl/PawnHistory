using System.Collections.Generic;
using Verse;

namespace PawnHistory.Source.PawnTracker
{
    public struct HistoryRecord(PawnEventDef eventDef)
    {
        public PawnEventDef EventDef { get; private set; } = eventDef;
        public long Date { get; private set; } = GenTicks.TicksAbs;
    }

    public class CompHistory : ThingComp
    {
        public List<HistoryRecord> records = [];
    }
}
