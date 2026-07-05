using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class WoundInfectionRecorder : RecorderBase<WoundInfectionEvent>
{
    private const float GuaranteedInfectionSeverity = 12f;

    public override void Register()
    {
        GameEventBus.Subscribe<WoundInfectionEvent>(CreateRecord);
    }

    public override void CreateRecord(WoundInfectionEvent e)
    {
        var (infection, sourceWound) = e;
        var pawn = sourceWound.pawn;
        if (!ShouldRecord(pawn))
            return;

        var recordDef = HistoryRecordDefOf.WoundInfection;
        var combatLogText = HasMatchingCombatLogText(sourceWound) ? sourceWound.combatLogText : null;
        var instigator = sourceWound.TryGetComp<HediffComp_History>()?.instigator;
        var scariaSource = infection.def == HediffDefOf.ScariaInfection ? instigator as Pawn : null;
        var desc = recordDef.Description(pawn)
            .IncludePawnGrammar()
            .AddRule("CombatLog", combatLogText)
            .AddRule("Infection", infection.LabelNoun())
            .AddRule("Part", infection.Part)
            .AddRule("SourceWound", sourceWound.LabelNounPretty())
            .AddRule("ScariaAnimal", scariaSource)
            .AddConstant("hasCombatLog", !combatLogText.NullOrEmpty())
            .AddConstant("hasScariaSource", scariaSource != null)
            .AddConstant("woundDef", sourceWound.def.defName)
            .Resolve();

        AddRecord(recordDef, pawn, desc, [instigator]);
    }

    // A damage log can describe another wound from the same hit if the damage affects multiple body parts.
    // Use combat text only when it names this source wound's part.
    // Core\Defs\RulePackDefs\RulePacks_CombatMelee.xml
    private static bool HasMatchingCombatLogText(Hediff sourceWound)
    {
        return sourceWound.combatLogText?.IndexOf(sourceWound.Part.Label, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public Action TestCombatLog(TestScenario scenario)
    {
        var friends = scenario.RaidFriendly().Point(700).RaidNeverFlee().Execute();
        var enemies = scenario.Incident(IncidentDefOf.RaidEnemy).Point(500).RaidNeverFlee().Execute();
        var pawns = scenario.Pawn(friends.Concat(enemies))
            .ThatMatches(ShouldRecord)
            .Execute();

        scenario.SpeedUp();
        scenario.Loop(data =>
        {
            var sourceWound = pawns
                .Where(p => !p.Dead)
                .SelectMany(p => p.health.hediffSet.hediffs)
                .FirstOrDefault(h => HasMatchingCombatLogText(h) && h.TryGetComp<HediffComp_Infecter>() != null);
            if (sourceWound == null)
                return;

            data.Cancelled = true;
            ForceInfection(sourceWound);
            scenario.OpenHistoryRecordTab(sourceWound.pawn);
        }, interval: 10, timeout: 2500);

        Expect.ThatAny(pawns).Eventually(2500).ToHaveHistoryRecord(HistoryRecordDefOf.WoundInfection, "[CombatLog] Later, [PAWN] developed [Infection] in [His] [Part].");

        return () => scenario.SlowDown();
    }

    public Action TestNonCombatWound(TestScenario scenario)
    {
        var victim = scenario.Pawn().ThatMatches(ShouldRecord).FullHeal().CreateSingle();
        Find.CurrentMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(Find.CurrentMap, victim.Position));

        scenario.SpeedUp();
        scenario.Loop(data =>
        {
            var sourceWound = victim.health.hediffSet.hediffs.FirstOrDefault(h => h.TryGetComp<HediffComp_Infecter>() != null);
            if (sourceWound == null)
                return;
        
            data.Cancelled = true;
            ForceInfection(sourceWound);
        }, interval: 10, timeout: 2500);

        Expect.That(victim).Eventually(2500).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.WoundInfection,
            Description = "[PAWN] developed [Infection] from [SourceWound].",
            Concerns = []
        });

        return () => scenario.SlowDown();
    }

    public void TestScariaInfection(TestScenario scenario)
    {
        var victim = scenario.Pawn().ThatMatches(ShouldRecord).FullHeal().CreateSingle();
        var scariaAttacker = scenario.Pawn().Animal(PawnKindDefOf.Megascarab).AddHediff(HediffDefOf.Scaria).CreateSingle();
        var sourceWound = CreateScariaSourceWound(victim, scariaAttacker);

        ForceInfection(sourceWound);

        Expect.That(victim).ToHaveHistoryRecord(HistoryRecordDefOf.WoundInfection, "[PAWN] developed a scaria infection in [His] [Part] after being bitten by a megascarab.");
    }

    private static Hediff CreateScariaSourceWound(Pawn victim, Pawn scariaAttacker)
    {
        var part = victim.GetBodyPart(BodyPartDefOf.Torso);
        var result = victim.TakeDamage(new DamageInfo(DamageDefOf.Bite, 1f, instigator: scariaAttacker, hitPart: part));
        var wound = result.hediffs.FirstOrDefault(h => h.TryGetComp<HediffComp_Infecter>() != null);

        Expect.That(wound).NotNull();
        Expect.That(wound.TryGetComp<HediffComp_Infecter>().fromScaria).True();

        return wound;
    }

    private static void ForceInfection(Hediff sourceWound)
    {
        var infecter = sourceWound.TryGetComp<HediffComp_Infecter>();
        var oldChanceFactor = Find.Storyteller.difficulty.playerPawnInfectionChanceFactor;

        try
        {
            sourceWound.Severity = GuaranteedInfectionSeverity;
            Find.Storyteller.difficulty.playerPawnInfectionChanceFactor = 1f;
            Accessor.HediffComp_Infecter.TicksUntilInfect(infecter) = 1;
            sourceWound.PostTickInterval(1);
        }
        finally
        {
            Find.Storyteller.difficulty.playerPawnInfectionChanceFactor = oldChanceFactor;
        }
    }
}
