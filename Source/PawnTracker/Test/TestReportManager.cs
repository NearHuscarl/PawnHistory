using System;
using System.Diagnostics;
using System.IO;
using PawnHistory.Source.Helper;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public static class TestReportManager
{
    private static readonly string FolderPath = Accessor.GenFilePaths.FolderUnderSaveData("PawnHistory");
    private static readonly string FilePath = Path.Combine(FolderPath, "LastTestRun.xml");
    
    private static readonly TestReport CurrentReport = new();
    public static TestReport LastReport { get; private set; } = LoadLastReport();

    public static void Reset()
    {
        CurrentReport.Entries.Clear();
    }
    
    public static void AddReportEntry(TestReportEntry reportEntry)
    {
        CurrentReport.Entries.Add(reportEntry);
    }

    public static void PrintReport()
    {
        var delta = CurrentReport.TimestampEnded - CurrentReport.TimestampStarted;
        var elapsed = TimeSpan.FromSeconds((double)delta / Stopwatch.Frequency);
        var time = elapsed.Hours > 0
            ? $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}.{elapsed.Milliseconds:D3}"
            : $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}.{elapsed.Milliseconds:D3}";

        Log.Message($"[PawnHistory] All tests finished in {time}. {Palette.Green($"Passed: {CurrentReport.AssertionsPassed}")}, {Palette.Red($"Failed: {CurrentReport.TestFailures}")}");
    }
    
    public static void SaveReport()
    {
        try
        {
            var newReport = LastReport.Upsert(CurrentReport);
            Scribe.saver.InitSaving(FilePath, "TestReport");
            Scribe_Deep.Look(ref newReport, "Report");
            Scribe.saver.FinalizeSaving();
            Log.Message($"[PawnHistory] Test report saved: {FilePath}");
            
            LastReport = newReport;
        }
        catch (Exception ex)
        {
            Log.Error($"[PawnHistory] Failed to save test report to {FilePath}: {ex}");
        }
    }

    public static TestReport LoadLastReport()
    {
        if (!File.Exists(FilePath))
        {
            Log.Warning($"[PawnHistory] Cannot load last test report because file not found: {FilePath}");
            return new TestReport();
        }

        try
        {
            TestReport lastReport = null;
            Scribe.loader.InitLoading(FilePath);
            Scribe_Deep.Look(ref lastReport, "Report");
            Scribe.loader.FinalizeLoading();
            return lastReport;
        }
        catch (Exception ex)
        {
            Log.Error($"[PawnHistory] Failed to load test report at {FilePath}: {ex}");
            return null;
        }
    }
}