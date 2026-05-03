using System;
using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace PawnHistory.Source.PawnTracker.Ui;

public class ITab_Pawn_History : ITab
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

    public override bool IsVisible => HistoryTableController.GetVisibleRecords(PawnToShowInfo).Any();

    public ITab_Pawn_History()
    {
        this.size = new Vector2(DefaultWidth, DefaultHeight);
        this.labelKey = "TabHistory";
        this.tutorTag = "History";
    }

    protected override void FillTab()
    {
        var pawn = PawnToShowInfo;
        var tabRect = new Rect(0, 0, size.x, size.y);
        historyCardPage.Draw(tabRect, pawn);
    }
}
