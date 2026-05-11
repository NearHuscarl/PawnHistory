using PawnHistory.Source.DebugTools;

namespace PawnHistory.Source.PawnTracker.Ui;

internal static class HistoryCardLayout
{
    public static float FilterHeight;
    public static float HeaderHeight;
    public static float MinRowHeight;
    public static float ColGap;
    public static float ColWidthDate;
    public static float ColWidthIcon;
    public static float CellPx;
    public static float ScrollWidth;
    public static float ControlWidth;
    public static float PageInputWidth;
    public static int PageSize;

    static HistoryCardLayout() => Reload();

    [Reloadable]
    [NearDebugAction]
    private static void Reload()
    {
        FilterHeight = 30f;

        HeaderHeight = 25f;
        MinRowHeight = 32f;
        ColGap = 5f;
        ColWidthDate = 90f;
        ColWidthIcon = 20f;
        CellPx = 5f;
        ScrollWidth = 16f;

        ControlWidth = 24f;
        PageInputWidth = 42f;
        PageSize = 12;
    }
}
