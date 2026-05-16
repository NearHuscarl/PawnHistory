using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public abstract class RaidComp : RecordComp<RaidRecorder>
{
    public record BuildInput(Pawn Pawn, Faction Faction, Quest Quest, Pawn QuestAsker);

    public abstract bool Match(BuildInput input);

    public virtual HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input)
    {
        return builder;
    }

    public virtual IEnumerable<Thing> GetConcerns(BuildInput input)
    {
        yield break;
    }

    public static string GenericRaidDescription = "[PAWN][AndOthers] from [Faction] raided the colony";
}
