using PawnHistory.Source.Helper;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.WorldPawn
{
    [StaticConstructorOnStartup]
    public class PawnColumnWorker_History : PawnColumnWorker_Icon
    {
        private static readonly Texture2D LogIcon = ContentFinder<Texture2D>.Get("ButtonIcons/History");
        private static readonly Texture2D EmptyLogIcon = ContentFinder<Texture2D>.Get("ButtonIcons/HistoryEmpty");

        protected override Texture2D GetIconFor(Pawn pawn) => pawn.GetHistoryRecords().Any() ? LogIcon : EmptyLogIcon;

        protected override void ClickedIcon(Pawn pawn)
        {
            Find.WindowStack.Add(new InfoCard(pawn, InfoCardType.History));
        }

        protected override string GetIconTip(Pawn pawn)
        {
            return "TabHistory".Translate();
        }
    }
}
