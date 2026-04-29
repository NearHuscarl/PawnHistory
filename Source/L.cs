using PawnHistory.Source.DebugTools;
using Verse;

namespace PawnHistory.Source;

public static class L
{
    public static void Debug(string message)
    {
        if (NearDebugSettings.LogDebug)
            Log.Message("[PawnHistory] " + message);
    }
    
    public static void Message(string message)
    {
        Log.Message("[PawnHistory] " + message);
    }
}