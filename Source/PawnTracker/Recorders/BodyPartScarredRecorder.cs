using System;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class BodyPartScarredRecorder : RecorderBase<BodyPartScarredEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<BodyPartScarredEvent>(CreateRecord);
    }

    public override void CreateRecord(BodyPartScarredEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        var (pawn, hediff, part, instigatorThing, reason) = e;
        var instigator = instigatorThing as Pawn; // TODO: handle thing
        var dmgSource = hediff.GetDamageSource();
        var recordDef = HistoryRecordDefOf.BodyPartScarred;
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .AddRule("Part", part.Label.Colorize(hediff.LabelColor))
            .AddRule("Hediff", hediff, addSubsymbols: true) // <permanentLabel>
            .AddRule("Instigator", instigator)
            .AddConstant("hasInstigator", instigator != null)
            .AddRule("DmgSource", dmgSource)
            .AddConstant("hasDmgSource", dmgSource != null)
            .AddConstant("reason", reason)
            .Resolve();

        AddRecord(recordDef, pawn, desc, [instigator]);
    }

    [SkipTest]
    public Action TestInjury(TestScenario scenario)
    {
        TestManager.Scenario.ForceInjuryScar = true;
        var friends = scenario.RaidFriendly()
            .Point(600)
            .Execute();

        var enemies = scenario.Incident(IncidentDefOf.RaidEnemy)
            .Point(500)
            .Execute();

        scenario.Pawn(friends.Concat(enemies))
            .ThatMatches(ShouldRecord)
            .FullHeal()
            .Execute();

        scenario.SpeedUp();

        return () => scenario.SlowDown();
    }

    [SkipTest]
    public Action TestPostHeal(TestScenario scenario)
    {
        TestManager.Scenario.ForcePostHealScar = true;
        var friends = scenario.RaidFriendly()
            .Point(600)
            .Execute();

        var enemies = scenario.Incident(IncidentDefOf.RaidEnemy)
            .Point(500)
            .Execute();

        scenario.Pawn(friends.Concat(enemies))
            .ThatMatches(ShouldRecord)
            .Execute();

        scenario.SpeedUp();

        return () => scenario.SlowDown();
    }
}
