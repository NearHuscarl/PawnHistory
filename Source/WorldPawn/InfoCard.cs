using PawnHistory.Source.PawnTracker.Ui;
using PawnHistory.Source.Ui;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.WorldPawn;

public enum InfoCardType
{
    Bio,
    Health,
    History
}

class InfoCard(Pawn pawn, InfoCardType infoType) : Window
{
    private readonly InfoCardType infoType = infoType;
    private readonly Pawn pawn = pawn;
    private readonly HistoryCardPage historyCardPage = new();
    private readonly WidgetHost widgets = new();

    public override Vector2 InitialSize => new(670f, 500f);

    public override void DoWindowContents(Rect inRect)
    {
        switch (infoType)
        {
            case InfoCardType.Bio:
                CharacterCardUtility.DrawCharacterCard(inRect.ContractedBy(18f), pawn);
                break;
            case InfoCardType.Health:
                HealthCardUtility.DrawPawnHealthCard(inRect.ContractedBy(18f), pawn, false, HealthCardUtility.ShowBloodLoss(pawn), null);
                break;
            case InfoCardType.History:
                var tabRect = new Rect(0, 0, ITab_Pawn_History.DefaultWidth, ITab_Pawn_History.DefaultHeight);
                widgets.Draw(tabRect, ctx => historyCardPage.Build(ctx, pawn));
                break;
        }
    }
}
