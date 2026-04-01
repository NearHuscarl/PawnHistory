using PawnHistory.Source.DebugTools;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class BodyPartScarredRecorder : RecorderBase
{
    public override void Register()
    {
        GameEventBus.Subscribe<BodyPartScarredEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn))
                return;

            HandleScarredPartEvent(e);
        });
    }

    private void HandleScarredPartEvent(BodyPartScarredEvent e)
    {
        var pawn = e.Pawn;
        var hediff = e.Hediff;
        var part = e.Part;

        var instigator = e.Instigator as Pawn;
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
            .AddConstant("reason", e.Reason)
            .Resolve();

        AddRecord(recordDef, pawn, desc, [instigator]);
    }

    public void TestInjury(TestScenario scenario)
    {
        NearDebugSettings.ForceInjuryScar = true;
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
        scenario.RunOnceOn<LordToilChangeEvent>(e => e.NextToil is LordToil_PanicFlee, e => scenario.SlowDown());
    }

    public void TestPostHeal(TestScenario scenario)
    {
        NearDebugSettings.ForcePostHealScar = true;
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
        scenario.RunOnceOn<LordToilChangeEvent>(e => e.NextToil is LordToil_PanicFlee, e =>
        {
            scenario.Pawn(friends.Concat(enemies))
                .TendInjuries()
                .Execute();
            NearDebugSettings.ForcePostHealScar = false;
        });
    }
}
