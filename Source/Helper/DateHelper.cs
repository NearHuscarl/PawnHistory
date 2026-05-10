using RimWorld;
using Verse;

namespace PawnHistory.Source.Helper;

public static class DateHelper
{
    public static string GetShortDate(int date, int tileId)
    {
        var position = Find.WorldGrid.LongLatOf(WorldGridUtility.Tile(tileId));
        var hourInt = GenDate.HourInteger(date, position.x);
        var hour = $"{hourInt}h";

        if (Prefs.TwelveHourClockMode)
        {
            var ampm = hourInt >= 12 ? "PM" : "AM";
            hourInt %= 12;
            if (hourInt == 0) hourInt = 12;
            hour = $"{hourInt} {ampm}";
        }

        var day = GenDate.DayOfYear(date, position.x) + 1;
        var year = GenDate.Year(date, position.x);
        return $"Y{year} D{day} {hour}";
    }
}