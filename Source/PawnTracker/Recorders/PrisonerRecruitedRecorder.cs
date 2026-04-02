using PawnHistory.Source.DebugTools;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class PrisonerRecruitedRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<PrisonerRecruitedEvent>(e =>
        {
            HandlePrisonerRecruitedEvent(e);
        });
    }

    private void HandlePrisonerRecruitedEvent(PrisonerRecruitedEvent e)
    {
        var recordDef = HistoryRecordDefOf.PrisonerRecruited;
        // remove Sentence_RecruitAttemptAccepted
        var recruitAttemptText = e.LogEntryText.Split('.').Select(p => p.Trim()).FirstOrDefault(p => !p.NullOrEmpty());
        var desc = recordDef.Description(e.Prisoner, "Prisoner")
            .AddRule("Recruiter", e.Recruiter, addSubsymbols: true)
            .AddRule("Faction", e.Recruiter.Faction)
            .AddRule("InteractionLog", recruitAttemptText)
            .AddConstant("hasFactionName", e.Recruiter.Faction.HasName)
            .Resolve();

        if (ShouldRecord(e.Recruiter))
            AddRecord(recordDef, e.Recruiter, desc, [e.Prisoner]);
        if (ShouldRecord(e.Prisoner))
            AddRecord(recordDef, e.Prisoner, desc, [e.Recruiter]);
    }

    public override void Test(TestScenario scenario)
    {
        NearDebugSettings.NoDisabledWorkTypes = true;
        DebugSettings.instantRecruit = true;

        var prisoners = new List<Pawn>();

        scenario.Thing()
            .BuildRoom(8, 8)
            .AsPrison(2, prisoners: prisoners)
            .Execute();
        var recruiter = scenario.Pawn()
            .Colonist()
            .StartJob(JobDefOf.PrisonerAttemptRecruit, prisoners[0])
            .CreateSingle();

        scenario.SpeedUp();

        GameEventBus.SubscribeOnce<PrisonerRecruitedEvent>(e =>
        {
            NearDebugSettings.NoDisabledWorkTypes = false;
            DebugSettings.instantRecruit = false;
            scenario.SlowDown();
            scenario.OpenHistoryRecordTab(recruiter);
        });
    }

    public void TestNamedFaction(TestScenario scenario)
    {
        NearDebugSettings.NoDisabledWorkTypes = true;
        DebugSettings.instantRecruit = true;

        var prisoners = new List<Pawn>();

        scenario.Thing()
            .BuildRoom(8, 8)
            .AsPrison(2, prisoners: prisoners)
            .Execute();
        var recruiter = scenario.Pawn()
            .Colonist()
            .StartJob(JobDefOf.PrisonerAttemptRecruit, prisoners[0])
            .CreateSingle();

        recruiter.Faction.Name = "Deez Nuts";
        scenario.SpeedUp();

        GameEventBus.SubscribeOnce<PrisonerRecruitedEvent>(e =>
        {
            NearDebugSettings.NoDisabledWorkTypes = false;
            DebugSettings.instantRecruit = false;
            scenario.SlowDown();
            scenario.OpenHistoryRecordTab(recruiter);
        });
    }
}
