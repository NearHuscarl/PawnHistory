using System;
using System.IO;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public static class TestReportManager
{
    private static readonly string FolderPath = Accessor.GenFilePaths.FolderUnderSaveData("PawnHistory");
    private static readonly string FilePath = Path.Combine(FolderPath, "LastTestRun.xml");
    
    private static TestReport currentReport = new();
    
    public static void Reset()
    {
        currentReport.Entries.Clear();
    }
    
    public static void AddReportEntry(TestReportEntry reportEntry)
    {
        currentReport.Entries.Add(reportEntry);
    }
    
    public static void SaveReport()
    {
        try
        {
            Scribe.saver.InitSaving(FilePath, "TestReport");
            Scribe_Deep.Look(ref currentReport, "Report");
            Scribe.saver.FinalizeSaving();
            Log.Message($"[PawnHistory] Report saved to XML: {FilePath}");
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
            return null;
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