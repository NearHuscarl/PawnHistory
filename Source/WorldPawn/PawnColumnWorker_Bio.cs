using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.WorldPawn
{
    [StaticConstructorOnStartup]
    public class PawnColumnWorker_Bio : PawnColumnWorker_Icon
    {
        protected override Texture2D GetIconFor(Pawn pawn) => TexButton.Info;

        protected override void ClickedIcon(Pawn pawn)
        {
            Find.WindowStack.Add(new InfoCard(pawn, InfoCardType.Bio));
        }

        protected override string GetIconTip(Pawn pawn)
        {
            return "TabCharacter".Translate();
        }
    }
}
