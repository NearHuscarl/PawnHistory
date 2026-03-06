using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker;

[HarmonyPatch(typeof(IncidentWorker_TraderCaravanArrival), "SendLetter")]
public static class IncidentWorker_TraderCaravanArrival_Patch
{
    public static void Postfix(IncidentParms parms, List<Pawn> pawns, TraderKindDef traderKind)
    {
        pawns = pawns.Where(PawnTracker.ShouldTrack).ToList();

        var eventDef = PawnEventDefOf.TradeCaravanArrived;
        GameEventListener.Publish(new GroupEvent(pawns, pawns[0].Faction, eventDef, pawn =>
        {
            return eventDef.description.Formatted(
                pawn.NameShortColored.Named("PAWN"),
                traderKind.label.Named("TRADERKIND"),
                pawns[0].Faction.NameColored.Named("FACTION")
            );
        }));
    }
}
