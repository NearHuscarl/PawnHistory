using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class IncidentBuilder
{
    private readonly IncidentDef def;
    private readonly IncidentParms parms;
    private readonly Map map;

    public IncidentBuilder(IncidentDef def)
    {
        this.def = def;
        map = Find.CurrentMap;
        parms = StorytellerUtility.DefaultParmsNow(def.category, map);
        parms.forced = true;
    }

    public IncidentBuilder PawnCount(int count)
    {
        parms.pawnCount = count;
        return this;
    }

    public IncidentBuilder Faction(Faction faction)
    {
        parms.faction = faction;
        return this;
    }

    /// <summary>
    /// Executes the incident and returns the list of pawns it spawned.
    /// </summary>
    public List<Pawn> Execute()
    {
        var oldLords = map.lordManager.lords.ToList();
        var oldPawns = map.mapPawns.AllPawnsSpawned.ToList();

        if (!def.Worker.TryExecute(parms))
        {
            Log.Warning($"Incident {def.defName} failed to execute.");
            return [];
        }

        var newLord = map.lordManager.lords.Except(oldLords).FirstOrDefault();
        if (newLord != null)
            return newLord.ownedPawns;

        return map.mapPawns.AllPawnsSpawned.Except(oldPawns).ToList();
    }
}