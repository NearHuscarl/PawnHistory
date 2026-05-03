using System.Collections.Generic;
using PawnHistory.Source.Helper;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.PawnTracker.Ui;

internal static class HistoryCardMenuOptions
{
    private static void CopyDescriptionToClipboard(HistoryRecord record)
    {
        GUIUtility.systemCopyBuffer = LangUtility.StripColorTags(record.description);
        Messages.Message("NH_PH_HistoryCard_RecordCopied".Translate(), MessageTypeDefOf.NeutralEvent);
    }

    private static void TogglePinned(HistoryRecord record) => record.pinned = !record.pinned;

    public static List<FloatMenuOption> GetActionMenuOptions(HistoryRecord record)
    {
        return [
            new FloatMenuOption("NH_PH_HistoryCard_MenuCopyDescription".Translate(), () => CopyDescriptionToClipboard(record)),
            new FloatMenuOption("NH_PH_HistoryCard_MenuEdit".Translate(), null), // TODO: edit record, support color tag
            new FloatMenuOption((record.pinned ? "NH_PH_HistoryCard_MenuUnpin" : "NH_PH_HistoryCard_MenuPin").Translate(), () => TogglePinned(record))
        ];
    }
}