using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;


namespace PawnHistory.Source.WorldPawn;

public class MainTabWindow_WorldPawns : MainTabWindow_PawnTable
{
    private List<Pawn> worldPawns = [];

    public MainTabWindow_WorldPawns()
    {
        worldPawns = Find.World.worldPawns.AllPawnsAliveOrDead.ToList();
    }

    protected override PawnTableDef PawnTableDef => PawnTableDefOf.WorldPawnTracker_MainTable;

    protected override IEnumerable<Pawn> Pawns => worldPawns;

    protected override float ExtraTopSpace => 5;

    public override void DoWindowContents(Rect rect)
    {
        base.DoWindowContents(rect);
    }

    public override void PostOpen()
    {
        base.PostOpen();
    }
}
