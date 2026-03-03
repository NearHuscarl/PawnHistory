using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PawnHistory.Source.WorldPawn
{
    public class PawnColumnWorker_WorldSituation : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
            => Find.World.worldPawns.GetSituation(pawn).ToString();
    }
}
