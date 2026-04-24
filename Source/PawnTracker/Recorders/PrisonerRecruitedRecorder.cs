using System;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class PrisonerRecruitedRecorder : RecorderBase<PrisonerRecruitedEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<PrisonerRecruitedEvent>(CreateRecord);
    }

    public override void CreateRecord(PrisonerRecruitedEvent e)
    {
        var recordDef = HistoryRecordDefOf.PrisonerRecruited;
        // remove Sentence_RecruitAttemptAccepted
        var recruitAttemptText = e.LogEntryText.Split('.').Select(p => p.Trim()).FirstOrDefault(p => !p.NullOrEmpty());
        var desc = recordDef.Description(e.Prisoner, "Prisoner")
            .WithPlayerFaction()
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

    public Action Test(TestScenario scenario)
    {
        DebugSettings.instantRecruit = true;
        scenario.SpeedUp();

        var prisoners = new List<Pawn>();

        scenario.Map()
            .BuildRoom(8, 8)
            .AsPrison(2, prisoners: prisoners)
            .Execute();
        var recruiter = scenario.Pawn()
            .Colonist()
            .FullHeal()
            .StartJob(JobDefOf.PrisonerAttemptRecruit, prisoners[0])
            .CreateSingle();

        recruiter.Faction.Name = null;

        Expect.That(recruiter)
            .Eventually()
            .ToHaveHistoryRecord("[InteractionLog]. [Prisoner] accepted and joined the colony.");

        return () =>
        {
            DebugSettings.instantRecruit = false;
            scenario.SlowDown();
            scenario.OpenHistoryRecordTab(recruiter);
        };
    }

    public Action TestNamedFaction(TestScenario scenario)
    {
        DebugSettings.instantRecruit = true;
        scenario.SpeedUp();

        var prisoners = new List<Pawn>();

        scenario.Map()
            .BuildRoom(8, 8)
            .AsPrison(2, prisoners: prisoners)
            .Execute();
        var recruiter = scenario.Pawn()
            .Colonist()
            .FullHeal()
            .StartJob(JobDefOf.PrisonerAttemptRecruit, prisoners[0])
            .Do(p => p.Faction.Name = "Deez Nuts")
            .CreateSingle();

        Expect.That(recruiter)
            .Eventually()
            .ToHaveHistoryRecord("[InteractionLog]. [Prisoner] accepted and joined Deez Nuts.");
        
        return () =>
        {
            DebugSettings.instantRecruit = false;
            scenario.SlowDown();
            scenario.OpenHistoryRecordTab(recruiter);
        };
    }
}
