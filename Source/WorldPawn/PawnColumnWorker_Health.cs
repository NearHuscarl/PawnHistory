using PawnHistory.Source.Helper;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.WorldPawn
{
    [StaticConstructorOnStartup]
    public class PawnColumnWorker_Health : PawnColumnWorker_Icon
    {
        private readonly Texture2D icon = ContentFinder<Texture2D>.Get("UI/Icons/Trainables/Rescue");

        protected override Texture2D GetIconFor(Pawn pawn) => icon;

        protected override Color GetIconColor(Pawn pawn)
        {
            return HediffHelper.VisibleHediffs(pawn).Any(h => h.def.isBad) ? Color.red : Color.grey;
        }

        protected override void ClickedIcon(Pawn pawn)
        {
            Find.WindowStack.Add(new InfoCard(pawn, InfoCardType.Health));
        }

        protected override string GetIconTip(Pawn pawn)
        {
            return HediffHelper.GetHediffText(pawn);
        }

        public override int Compare(Pawn a, Pawn b)
            => HediffHelper.VisibleHediffs(a).Count().CompareTo(HediffHelper.VisibleHediffs(b).Count());
    }
}
