using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class Surgery_ImplantRecorder : SurgeryRecorder
{
    public override void Register()
    {
        GameEventBus.Subscribe<SurgeryInstallImplantEvent>(e =>
        {
            if (e.Outcome.failure)
            {
                var botched = HistoryRecordDefOf.BodyPartImplanted.Description(e.Patient)
                    .AddRule("ImplantHediff", e.HediffToAdd, addSubsymbols: true)
                    .Resolve("BotchedSurgery")
                    .ToLower();
                HandleBotchSurgeryEvent(e, botched);
            }
            else
                HandleBodyPartImplantedEvent(e);
        });
    }

    private void HandleBodyPartImplantedEvent(SurgeryInstallImplantEvent e)
    {
        if (!ShouldRecord(e.Patient))
            return;

        var recordDef = HistoryRecordDefOf.BodyPartImplanted;
        var desc = recordDef.Description(e.Patient)
            .AddRule("Doctor", e.Doctor)
            .AddRule("ImplantHediff", e.HediffToAdd, addSubsymbols: true)
            .AddRule("EnhancedPart", e.Part)
            .Resolve();
        AddRecord(recordDef, e.Patient, desc, [e.Doctor]);
    }

    public override void Test(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing("Joywire", 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor()
            .Heal()
            .DoSurgery(patient, DefDatabase<RecipeDef>.GetNamed("InstallJoywire"), DefDatabase<BodyPartDef>.GetNamed("Brain"))
            .CreateSingle();
    }

    public void TestFail(TestScenario scenario)
    {
        scenario.Map()
            .BuildRoom(6, 6, tag: "Hospital")
            .AsHospital(bedCount: 2)
            .WithThing("Joywire", 1)
            .Execute();

        var patient = scenario.Pawn()
            .Colonist()
            .CreateSingle();

        scenario.Pawn()
            .Colonist()
            .SetDoctor(isBadDoctor: true)
            .DoSurgery(patient, DefDatabase<RecipeDef>.GetNamed("InstallJoywire"), DefDatabase<BodyPartDef>.GetNamed("Brain"), instant: true)
            .CreateSingle();
    }
}