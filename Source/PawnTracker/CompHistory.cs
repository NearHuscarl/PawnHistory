using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public class CompHistory : ThingComp
{
    public List<HistoryRecord> records;
    
    public CompHistory() => EnsureInitialized();

    private void EnsureInitialized()
    {
        records ??= [];

        foreach (var record in records.ToList())
        {
            // remove corrupted records during development.
            if (record.pawn == null)
            {
                records.Remove(record);
            }
        }
    }

    public string GetShortDate(HistoryRecord record)
    {
        var location = parent.Tile.Valid ? Find.WorldGrid.LongLatOf(parent.Tile) : Vector2.zero;
        var hourInt = GenDate.HourInteger(record.date, location.x);
        var hour = $"{hourInt}h";

        if (Prefs.TwelveHourClockMode)
        {
            var ampm = hourInt >= 12 ? "PM" : "AM";
            hourInt %= 12;
            if (hourInt == 0) hourInt = 12;
            hour = $"{hourInt} {ampm}";
        }

        var day = GenDate.DayOfYear(record.date, location.x) + 1;
        var year = GenDate.Year(record.date, location.x);
        return $"Y{year} D{day} {hour}";
    }

    public string GetTipDate(HistoryRecord record)
    {
        var location = parent.Tile.Valid ? Find.WorldGrid.LongLatOf(parent.Tile) : Vector2.zero;
        return GenDate.DateFullStringWithHourAt(record.date, location);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Collections.Look(ref records, "historyRecords", LookMode.Deep);
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
            return;

        EnsureInitialized();
    }
}
