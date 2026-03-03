using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace PawnHistory.Source.WorldPawn
{
    public class PawnColumnWorker_ForceKept : PawnColumnWorker_Checkbox
    {
        public static float IconPositionVertical = 35f;
        public static float IconPositionHorizontal = 5f;

        protected override bool GetValue(Pawn pawn) => Find.World.worldPawns.ForcefullyKeptPawns.Contains(pawn);

        protected override bool HasCheckbox(Pawn pawn) => Find.World.worldPawns.Contains(pawn) && Find.World.worldPawns.GetSituation(pawn) != WorldPawnSituation.Dead;

        protected override void SetValue(Pawn pawn, bool value, PawnTable table)
        {
            if (value)
            {
                Find.World.worldPawns.ForcefullyKeptPawns.Add(pawn);
            }
            else
            {
                Find.World.worldPawns.ForcefullyKeptPawns.Remove(pawn);
            }
        }
    }
}
