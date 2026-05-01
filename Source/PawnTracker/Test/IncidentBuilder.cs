using PawnHistory.Source.DebugTools;
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

    public IncidentBuilder(IncidentDef def) : this(def, Find.CurrentMap) { }

    public IncidentBuilder(IncidentDef def, IIncidentTarget target)
    {
        this.def = def;
        map = Find.CurrentMap;
        parms = StorytellerUtility.DefaultParmsNow(def.category, target);
        parms.forced = true;
    }

    public IncidentBuilder Point(int point)
    {
        parms.points = point;
        return this;
    }

    public IncidentBuilder TraderKind(TraderKindDef traderKindDef)
    {
        parms.traderKind = traderKindDef;
        return this;
    }

    public IncidentBuilder RaidNeverFlee()
    {
        parms.raidNeverFleeIndividual = true;
        return this;
    }

    public IncidentBuilder RaidStrategy(RaidStrategyDef raidStrategy)
    {
        parms.raidStrategy = raidStrategy;
        return this;
    }

    public IncidentBuilder RaidArrivalMode(PawnsArrivalModeDef pawnsArrivalModeDef)
    {
        parms.raidArrivalMode = pawnsArrivalModeDef;
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
            Log.Warning($"Incident {def.defName} failed to execute. {DebugUtility.Format(parms)}");
            return [];
        }

        var newLord = map.lordManager.lords.Except(oldLords).FirstOrDefault();
        if (newLord != null)
            return newLord.ownedPawns;

        return map.mapPawns.AllPawnsSpawned.Except(oldPawns).ToList();
    }
}
