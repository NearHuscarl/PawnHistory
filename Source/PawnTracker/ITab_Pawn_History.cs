using RimWorld;
using System;
using UnityEngine;
using Verse;

#nullable disable
namespace PawnHistory.Source.PawnTracker;

public class ITab_Pawn_History : ITab
{
    public Pawn PawnToShowInfo
    {
        get
        {
            if (this.SelPawn != null)
                return this.SelPawn;
            return this.SelThing is Corpse corpse ? corpse.InnerPawn : throw new InvalidOperationException("History tab found no selected pawn to display.");
        }
    }

    public override bool IsVisible
    {
        get => CompHistoryManager.GetComp(PawnToShowInfo) != null;
    }

    public ITab_Pawn_History()
    {
        this.size = new Vector2(600f, 500f);
        this.labelKey = "TabHistory";
        this.tutorTag = "History";
    }

    public override void OnOpen()
    {
        base.OnOpen();
        HistoryCardUtility.scrollPosition = Vector2.zero;
    }

    protected override void FillTab()
    {
        var pawn = PawnToShowInfo;

        GUI.BeginGroup(HistoryCardUtility.HistoryRect);
        HistoryCardUtility.DrawHistoryCard(HistoryCardUtility.HistoryRect, pawn, CompHistoryManager.GetComp(pawn));
        GUI.EndGroup();
    }
}
