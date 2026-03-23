using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class SurgeryRecorder : RecorderBase
{
    enum SurgeryOutcomeType
    {
        Minor,
        Major,
        Death,
        Sterilized,
    }

    public override void Register() { }

    protected void HandleBotchSurgeryEvent(SurgeryEvent e, string botched)
    {
        var recordDef = HistoryRecordDefOf.BotchedSurgery;
        var injuredParts = e.NewInjuries.Select(h => h.Part).Distinct().ToList();
        var bloodloss = e.Patient.GetBloodlossText();
        var outcomeType = GetSurgeryOutcomeType(e.Outcome, e.Patient);
        var desc = recordDef.ResolveDescription("botchedSurgery", e.Patient)
            .IncludePawnGrammar()
            .AddRule("Doctor", e.Doctor)
            .AddRule("BotchedSurgery", botched)
            .AddRule("InjuredParts", LangUtility.FormatList(injuredParts, p => p.Label, "NH_PH_OtherPart".Translate()))
            .AddRule("Bloodloss", e.Patient.Dead ? "" : bloodloss)
            .AddConstant("outcomeType", outcomeType)
            .AddConstant("injuryCount", e.NewInjuries.Count)
            .Resolve();
        AddRecord(HistoryRecordDefOf.BotchedSurgery, e.Patient, desc, [e.Doctor]);
    }

    private SurgeryOutcomeType GetSurgeryOutcomeType(SurgeryOutcome outcome, Pawn pawn)
    {
        if (outcome is SurgeryOutcome_Death || pawn.Dead /* non-fatal failure could make a pawn dead */)
            return SurgeryOutcomeType.Death;
        else if (outcome is SurgeryOutcome_FailureWithHediff fwh && fwh.failedHediff == HediffDefOf.Sterilized)
            return SurgeryOutcomeType.Sterilized;
        else if (outcome.totalDamage > 50)
            return SurgeryOutcomeType.Major;
        else
            return SurgeryOutcomeType.Minor;
    }
}