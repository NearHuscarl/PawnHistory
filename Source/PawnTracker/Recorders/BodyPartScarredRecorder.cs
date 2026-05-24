using System;
using PawnHistory.Source.Helper;
using System.Collections.Generic;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.HistoryBackfill;
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

    internal override IEnumerable<HistoryBackfillDefinition> GetBackfillDefinitions()
    {
        const string densityGroup = "HealthPrehistory";

        yield return new HistoryBackfillDefinition(HistoryRecordDefOf.BodyPartScarred, densityGroup)
            .AddHard(
                new MinimumAgeRule(7f),
                new SiblingSequenceRule(GenDate.DaysToTicks(45f)))
            .AddSoft(new AgeCurveSoftRule([
                new CurvePoint(7f, 0.02f),
                new CurvePoint(13f, 0.12f),
                new CurvePoint(18f, 0.4f),
                new CurvePoint(30f, 1f),
                new CurvePoint(55f, 1.15f),
                new CurvePoint(90f, 0.6f)
            ]))
            .AddGlobal(new DensityGlobalRule(densityGroup));
    }

    public override void CreateRecord(BodyPartScarredEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        AddRecord(HistoryRecordDefOf.BodyPartScarred, e.Pawn, e, inputs =>
        {
            var e2 = inputs[0];
            var (pawn, hediff, part, instigatorThing, reason) = e2;
            var instigator = instigatorThing as Pawn; // TODO: handle thing
            var dmgSource = hediff.GetDamageSource();
            var recordDef = HistoryRecordDefOf.BodyPartScarred;
            var hediffs = inputs.Select(h => h.Hediff.LabelBase).Distinct().ToList();
            var scarList = LangUtility.FormatList(hediffs, h => Find.ActiveLanguageWorker.WithIndefiniteArticlePostProcessed(h).Colorize(hediff.LabelColor),
                otherText: "NH_PH_OtherScar".TranslateSimple());
            var desc = recordDef.Description(pawn)
                .IncludePawnGrammar()
                .AddRule("Part", part.Label.Colorize(hediff.LabelColor))
                .AddRule("Hediff", hediff, addSubsymbols: true) // <permanentLabel>
                .AddRule("Hediffs", scarList)
                .AddRule("Instigator", instigator)
                .AddConstant("hasInstigator", instigator != null)
                .AddRule("DmgSource", dmgSource)
                .AddConstant("hasDmgSource", !dmgSource.NullOrEmpty())
                .AddConstant("reason", reason)
                .Resolve();

            return new HistoryRecordWriteRequest(recordDef, pawn, desc, [instigator]);
        });
    }

    [SkipTest]
    public Action TestInjury(TestScenario scenario)
    {
        scenario.ForceInjuryScar = true;
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
        scenario.ForcePostHealScar = true;
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

    public void TestMultiple(TestScenario scenario)
    {
        scenario.ForceInjuryScar = true;

        // could happen in a catastrophic botched surgery
        var victim = scenario.Pawn().Colonist()
            .TakeDamage(1f, Extra.BodyPartDefOf.Brain)
            .TakeDamage(1f, Extra.BodyPartDefOf.Brain)
            .TakeDamage(1f, Extra.BodyPartDefOf.Brain)
            .TakeDamage(1f, Extra.BodyPartDefOf.Brain)
            .CreateSingle();
        
        Expect.That(victim).ToHaveTheLastHistoryRecordsOf([HistoryRecordDefOf.NewArrival, HistoryRecordDefOf.BodyPartScarred]);
    }
}
