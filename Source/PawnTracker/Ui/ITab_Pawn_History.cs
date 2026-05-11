using System;
using PawnHistory.Source.Helper;
using PawnHistory.Source.Ui;
using UnityEngine;
using Verse;

#nullable disable
namespace PawnHistory.Source.PawnTracker.Ui;

public class ITab_Pawn_History : WidgetTab
{
    public static readonly float DefaultWidth = 650f;
    public static readonly float DefaultHeight = 510f;
    private readonly HistoryCardPage historyCardPage = new();

    public Pawn PawnToShowInfo
    {
        get
        {
            if (this.SelPawn != null)
                return this.SelPawn;
            if (this.SelThing is Corpse corpse)
                return corpse.InnerPawn;
            throw new InvalidOperationException($"History tab found no selected pawn to display. Check {nameof(CompHistoryManager.AttachHistoryComp)} for regression.");
        }
    }

    public override bool IsVisible => RecorderManager.ShouldRecord(PawnToShowInfo) || PawnToShowInfo.VisibleHistoryRecords.Any();

    public ITab_Pawn_History()
    {
        this.size = new Vector2(DefaultWidth, DefaultHeight);
        this.labelKey = "TabHistory";
        this.tutorTag = "History";
    }

    protected override Widget Build(UiContext ctx)
    {
        return historyCardPage.Build(ctx, PawnToShowInfo);
    }
}
