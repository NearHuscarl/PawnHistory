using RimWorld;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PawnHistory.Source.PawnTracker.Test;

public class GatheringBuilderResult(Lord lord, Pawn organizer)
{
    public Lord Lord { get; } = lord;
    public Pawn Organizer { get; } = organizer;
}

public class GatheringBuilder(GatheringDef def)
{
    private readonly GatheringDef def = def;
    private readonly Map map = Find.CurrentMap;

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
        var gatheringJob = newLord.LordJob as LordJob_Joinable_Gathering;
        var organizer = gatheringJob.Organizer;

        return new GatheringBuilderResult(newLord, organizer);
    }
}