using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;


namespace PawnHistory.Source.WorldPawn
{
    public class MainTabWindow_WorldPawns : MainTabWindow_PawnTable
    {
        private List<Pawn> worldPawns = new List<Pawn>();

        public MainTabWindow_WorldPawns()
        {
            worldPawns = Find.World.worldPawns.AllPawnsAlive.ToList();
            Log.Message("[WorldPawnTracker] MainTabWindow_WorldPawns()");
        }

        protected override PawnTableDef PawnTableDef => PawnTableDefOf.WorldPawnTracker_MainTable;

        protected override IEnumerable<Pawn> Pawns
        {
            get
            {
                return worldPawns;
            }
        }

        protected override float ExtraTopSpace => 5;

        public override void DoWindowContents(Rect rect)
        {
            base.DoWindowContents(rect);
        }

        public override void PostOpen()
        {
            base.PostOpen();
            //Find.World.renderer.wantedMode = WorldRenderMode.None;
        }
    }
}
