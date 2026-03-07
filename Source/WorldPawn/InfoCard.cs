using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.WorldPawn;

public enum InfoCardType
{
    Bio,
    Health,
}

class InfoCard(Pawn pawn, InfoCardType infoType) : Window
{
    private readonly InfoCardType infoType = infoType;
    private readonly Pawn pawn = pawn;

    public override Vector2 InitialSize => new(650f, 500f);

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
        }
    }
}
