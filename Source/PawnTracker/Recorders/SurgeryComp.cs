using PawnHistory.Source.PawnTracker.Events;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public abstract class SurgeryComp : RecordComp<SurgeryRecorder>
{
    public record BuildInput(SurgeryEvent Event);

    public abstract bool Match(BuildInput input);

    public abstract HistoryRecordDef RecordDef(BuildInput input);

    public abstract HistoryDescriptionBuilder BuildGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input);

    public abstract HistoryDescriptionBuilder BuildBotchedGrammarRequest(HistoryDescriptionBuilder builder, BuildInput input);
}
