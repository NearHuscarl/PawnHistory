using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class TestReport : IExposable
{
    public List<TestReportEntry> Entries = [];
    public long TimestampStarted => Entries.FirstOrDefault()?.timestampStarted ?? 0;
    public long TimestampEnded  => Entries.LastOrDefault()?.timestampEnded ?? 0;
    public int AssertionsPassed  => Entries.Sum(e => e.AssertionsPassed);
    public int TestFailures  => Entries.Sum(e => e.TestFailures.Count);

    public TestReport Upsert(TestReport newReport)
    {
        if (newReport?.Entries == null)
            return this;

        var dict = Entries.ToDictionary(e => e.TestId);

        foreach (var incoming in newReport.Entries)
        {
            if (!dict.TryGetValue(incoming.TestId, out var existing))
            {
                dict[incoming.TestId] = incoming;
                continue;
            }

            if (incoming.date > existing.date)
                dict[incoming.TestId] = incoming;
        }

        return new TestReport
        {
            Entries = dict.Values.ToList()
        };
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref Entries, "Entries", LookMode.Deep);
    }
}

public class TestReportEntry : IExposable
{
    public string TestId;
    public int AssertionsPassed;
    public List<TestFailure> TestFailures;
    public long timestampStarted;
    public long timestampEnded;
    public long date;

    public TestReportEntry()
    {
        timestampStarted = Stopwatch.GetTimestamp();
        date = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref TestId, "TestId");
        Scribe_Values.Look(ref AssertionsPassed, "AssertionsPassed");
        Scribe_Collections.Look(ref TestFailures, "TestFailures");
        Scribe_Values.Look(ref timestampStarted, "timestampStarted");
        Scribe_Values.Look(ref timestampEnded, "timestampEnded");
        Scribe_Values.Look(ref date, "date");
    }
}
