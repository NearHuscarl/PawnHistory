using System.Collections.Generic;
using RimWorld;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Test;

public class GatheringBuilderResult(Lord lord, List<Pawn> organizers)
{
    public Lord Lord { get; } = lord;
    public List<Pawn> Organizers { get; } = organizers;
}

public class GatheringBuilder(GatheringDef def)
{
    private readonly GatheringDef def = def;
    private readonly Map map = Find.CurrentMap;

    private static List<Pawn> GetOrganizers(LordJob lordJob)
    {
        if (lordJob is LordJob_Joinable_Gathering g)
            return [g.Organizer];
        else if (lordJob is LordJob_Joinable_MarriageCeremony mc)
            return [mc.firstPawn, mc.secondPawn];
        return [];
    }

    /// <summary>
    /// Executes the incident and returns the list of pawns it spawned.
    /// </summary>
    public GatheringBuilderResult Execute()
    {
        var oldLords = map.lordManager.lords.ToList();
        var success = false;

        for (var attempt = 0; attempt < 20; attempt++)
        {
            success = Find.CurrentMap.lordsStarter.TryStartGathering(def);
            if (success)
                break;
        }
        if (!success)
        {
            Log.Warning($"Gathering {def.defName} failed to execute.");
            return new GatheringBuilderResult(null, null);
        }

        var newLord = map.lordManager.lords.Except(oldLords).FirstOrDefault();
        var organizers = GetOrganizers(newLord.LordJob);

        return new GatheringBuilderResult(newLord, organizers);
    }
}