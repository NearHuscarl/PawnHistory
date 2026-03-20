using PawnHistory.Source.DebugTools;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class BodyPartRemovedRecorder : RecorderBase
{
    enum SurgeryFailedType
    {
        Minor,
        Major,
        Death,
        Sterilized,
    }

    public override void Register()
    {
        GameEventBus.Subscribe<BodyPartRemoveEvent>(e =>
        {
            if (!ShouldRecord(e.Patient))
                return;

            if (e.Outcome.failure)
                HandleBotchSurgeryEvent(e);
            else
                HandleBodyPartRemovedEvent(e);
        });
    }

    private void HandleBotchSurgeryEvent(BodyPartRemoveEvent e)
    {
        var recordDef = HistoryRecordDefOf.BotchedSurgery;
        var operation = e.Intent.ToString().ToLowerInvariant();
        var injuredParts = e.NewInjuries.Select(h => h.Part).Distinct().ToList();
        var bloodloss = e.Patient.GetBloodlossText();
        var failedType = GetSurgeryFailedType(e.Outcome, e.Patient);
        var desc = recordDef.ResolveDescription("botchedSurgery", e.Patient)
            .IncludePawnGrammar()
            .AddRule("Doctor", e.Doctor)
            .AddRule("Op", operation, addSubsymbols: true)
            .AddRule("Part", e.Part.Label)
            .AddConstant("failedType", failedType)
            .AddRule("InjuredParts", LangUtility.FormatList(injuredParts, p => p.Label, "NH_PH_OtherPart".Translate()))
            .AddRule("Bloodloss", e.Patient.Dead ? "" : bloodloss)
            .AddConstant("injuryCount", e.NewInjuries.Count)
            .Resolve();
        AddRecord(recordDef, e.Patient, desc, [e.Doctor]);
    }

    private void HandleBodyPartRemovedEvent(BodyPartRemoveEvent e)
    {
        var recordDef = HistoryRecordDefOf.BodyPartRemoved;
        var desc = recordDef.ResolveDescription("bodyPartRemoved", e.Patient)
            .AddRule("Doctor", e.Doctor)
            .AddRule("Part", e.Part.Label.Colorize(HediffDefOf.MissingBodyPart.defaultLabelColor))
            .AddRule("BadHediff", e.BadHediff?.LabelNounFull())
            .AddConstant("intent", e.Intent)
            .Resolve();
        AddRecord(recordDef, e.Patient, desc, [e.Doctor]);
    }

    private SurgeryFailedType GetSurgeryFailedType(SurgeryOutcome outcome, Pawn pawn)
    {
        if (outcome is SurgeryOutcome_Death || pawn.Dead /* non-fatal failure could make a pawn dead */)
            return SurgeryFailedType.Death;
        else if (outcome is SurgeryOutcome_FailureWithHediff fwh && fwh.failedHediff == HediffDefOf.Sterilized)
            return SurgeryFailedType.Sterilized;
        else if (outcome.totalDamage > 50)
            return SurgeryFailedType.Major;
        else
            return SurgeryFailedType.Minor;
    }

    public void TestFail(TestScenario scenario)
    {
        var beds = new List<Building_Bed>();
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2, beds)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .FullHeal()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor(isBadDoctor: true)
            .AddHediff(HediffDefOf.MissingBodyPart, BodyPartDefOf.Arm)
            .AddHediff(HediffDefOf.MissingBodyPart, BodyPartDefOf.Eye)
            .AddHediff("SmokeleafHigh", BodyPartDefOf.Torso)
            .DoSurgery(patient, beds[0], RecipeDefOf.RemoveBodyPart, BodyPartDefOf.Lung, instant: true)
            .CreateSingle();
    }

    public void TestHarvest(TestScenario scenario)
    {
        var beds = new List<Building_Bed>();
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2, beds)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .FullHeal()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, beds[0], RecipeDefOf.RemoveBodyPart, BodyPartDefOf.Lung)
            .CreateSingle();
    }

    public void TestAmputate(TestScenario scenario)
    {
        var beds = new List<Building_Bed>();
        scenario.Thing()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2, beds)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .FullHeal()
            .AddHediff(HediffDefOf.WoundInfection, BodyPartDefOf.Leg, 0.8f)
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, beds[0], RecipeDefOf.RemoveBodyPart, BodyPartDefOf.Leg)
            .CreateSingle();
    }
}