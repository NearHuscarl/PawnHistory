using PawnHistory.Source.DebugTools;

namespace PawnHistory.Source.PawnTracker.Ui;

internal static class HistoryCardLayout
{
    public static float TopBarHeight;
    public static float HeaderHeight;
    public static float MinRowHeight;
    public static float ColWidthDate;
    public static float ControlWidth;
    public static float PageInputWidth;
    public static int PageSize;

    static HistoryCardLayout() => Reload();

    [Reloadable]
    [NearDebugAction]
    private static void Reload()
    {
        TopBarHeight = 30f;

        HeaderHeight = 25f;
        MinRowHeight = 32f;
        ColWidthDate = 90f;

        ControlWidth = 24f;
        PageInputWidth = 42f;
        PageSize = 12;
    }
}
