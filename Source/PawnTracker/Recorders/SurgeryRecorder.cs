using System;
using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using PawnHistory.Source.PawnTracker.Test.Mocks;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class SurgeryRecorder : RecorderBase<SurgeryEvent>
{
    private enum SurgeryOutcomeType
    {
        Minor,
        Major,
        Death,
        Sterilized,
    }

    public override void Register()
    {
        GameEventBus.Subscribe<SurgeryEvent>(CreateRecord);
    }

    public override void CreateRecord(SurgeryEvent e)
    {
        if (!ShouldRecord(e.Patient))
            return;

        var input = new SurgeryComp.BuildInput(e);
        var comp = Comps.OfType<SurgeryComp>().FirstOrDefault(c => c.Match(input));
        if (comp == null)
        {
            L.Warning($"Unsupported surgery recipe {e.Recipe?.defName}.");
            return;
        }

        if (e.Outcome?.failure == true)
        {
            RecordBotchedSurgery(input, comp);
            return;
        }

        var recordDef = comp.RecordDef(input);
        var builder = recordDef.Description(e.Patient)
            .IncludePawnGrammar()
            .AddRule("Doctor", e.Doctor)
            .AddRule("Part", e.Part)
            .AddConstant("hasPartPosition", e.Part?.IsOneOfMultipleParts);

        builder = comp.BuildGrammarRequest(builder, input);
        AddRecord(recordDef, e.Patient, builder.Resolve(), [e.Doctor]);
    }

    private void RecordBotchedSurgery(SurgeryComp.BuildInput input, SurgeryComp comp)
    {
        var e = input.Event;
        var recordDef = comp.RecordDef(input);
        var injuredParts = e.NewInjuries.Select(h => h.Part).Distinct().ToList();
        var outcomeType = GetSurgeryOutcomeType(e.Outcome, e.Patient);
        var builder = recordDef.Description(e.Patient)
            .IncludePawnGrammar()
            .AddRule("Doctor", e.Doctor)
            .AddRule("Part", e.Part)
            .AddRule("InjuredParts", LangUtility.FormatList(injuredParts, p => p.Label, "NH_PH_OtherPart".Translate()))
            .AddRule("Bloodloss", e.Patient.Dead ? "" : e.Patient.GetBloodLossText())
            .AddConstant("outcomeType", outcomeType)
            .AddConstant("injuryCount", e.NewInjuries.Count);

        builder = comp.BuildBotchedGrammarRequest(builder, input);

        AddRecord(HistoryRecordDefOf.BotchedSurgery, e.Patient, builder.Resolve("entryBotched"), [e.Doctor]);
    }

    private static SurgeryOutcomeType GetSurgeryOutcomeType(SurgeryOutcome outcome, Pawn pawn)
    {
        if (outcome is SurgeryOutcome_Death || pawn.Dead /* non-fatal failure could make a pawn dead */)
            return SurgeryOutcomeType.Death;
        if (outcome is SurgeryOutcome_FailureWithHediff fwh && fwh.failedHediff == HediffDefOf.Sterilized)
            return SurgeryOutcomeType.Sterilized;
        if (outcome?.totalDamage > 50)
            return SurgeryOutcomeType.Major;

        return SurgeryOutcomeType.Minor;
    }

    public static (Pawn patient, Pawn doctor) DoSurgery(TestScenario scenario, RecipeDef recipeDef, BodyPartDef bodyPart = null, Action<PawnBuilder> buildPatient = null, SurgeryOutcome surgeryOutcome = null)
    {
        scenario.SurgeryForcedOutcome = surgeryOutcome ?? SurgeryOutcomes.Success;
        scenario.Map()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .Execute();

        var patientBuilder = scenario.Pawn().Colonist();
        buildPatient?.Invoke(patientBuilder);
        var patient = patientBuilder.CreateSingle();

        var doctor = scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .DoSurgery(patient, recipeDef, bodyPart, instant: true)
            .CreateSingle();

        return (patient, doctor);
    }
}
