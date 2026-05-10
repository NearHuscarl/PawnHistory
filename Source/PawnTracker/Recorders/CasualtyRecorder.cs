using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class CasualtyRecorder : RecorderBase<CasualtyRecorder.KillInput>, IRecord<CasualtyRecorder.KilledOrDownedInput>
{
    // TODO: add instigator from combat log in cause transition log misses it. 
    public record KillInput(Pawn Subject, Pawn Initiator, string CombatLogText, string TransitionText, Pawn OriginalTargetPawn);
    public record KilledOrDownedInput(Pawn Subject, Pawn Initiator, CasualtyType Casualty, HediffDef CulpritHediff, string CombatLogText, string TransitionText, Pawn OriginalTargetPawn, bool IsLeader);

    public override void Register()
    {
        GameEventBus.Subscribe<CasualtyLogAddedEvent>(e =>
        {
            var isLeader = e.Subject.IsFactionLeader(); // cache leader before it is reassigned next tick

            // TODO: add context class to intercept DamageWorker.AssociateWithLog, remove the delay(0) hack, and test the order of death > royal title inheritance to if it's fixed. 
            // We intercept BattleLog.Add() but it runs before DamageWorker.AssociateWithLog() which is required to populate bodyPart data so we can get the
            // exact in-game combat log. But since DamageWorker.AssociateWithLog() is not always used we need to pick BattleLog.Add and fallback.
            TickDelayManager.Delay(0, () =>
            {
                Pawn originalTargetPawn = null;
                // Note: Do not rely on the order. Most of the time, transitionEntry is inserted before the damageResultEntry, but occasionally the opposite happens.
                // Luckily they happen on the same tick if the damage cause state transition.
                var lastDamageEntry = e.Battle.Entries.FirstOrDefault(l => l.Tick == e.TransitionEntry.Tick && l is LogEntry_DamageResult && l.Concerns(e.Subject)) as LogEntry_DamageResult;

                if (lastDamageEntry is BattleLogEntry_RangedImpact rangedEntry)
                    originalTargetPawn = Accessor.BattleLogEntry_RangedImpact.OriginalTargetPawn(rangedEntry);

                if (e.Casualty == CasualtyType.Killed)
                {
                    var combatLogText = lastDamageEntry?.ToGameStringFromPOV(e.Initiator);
                    var transitionText = e.TransitionEntry.ToGameStringFromPOV(e.Initiator);
                    CreateRecord(new KillInput(e.Subject, e.Initiator, combatLogText, transitionText, originalTargetPawn));
                }
                if (e.CulpritHediff != HediffDefOf.Anesthetic)
                {
                    var combatLogText = lastDamageEntry?.ToGameStringFromPOV(e.Subject);
                    var transitionText = e.TransitionEntry.ToGameStringFromPOV(e.Subject);
                    CreateRecord(new KilledOrDownedInput(e.Subject, e.Initiator, e.Casualty, e.CulpritHediff, combatLogText, transitionText, originalTargetPawn, isLeader));
                }
            });
        });
    }

    public override void CreateRecord(KillInput input)
    {
        var (subject, initiator, combatLogText, transitionText, originalTarget) = input;
        if (!ShouldRecord(initiator))
            return;

        var desc = $"{combatLogText} {transitionText}";
        AddRecord(HistoryRecordDefOf.Kill, initiator, desc, [subject, originalTarget]);
    }

    public void CreateRecord(KilledOrDownedInput input)
    {
        var (subject, initiator, casualty, culpritHediff, combatLogText, transitionText, originalTarget, isLeader) = input;
        if (!ShouldRecord(subject))
            return;

        var isKillLog = casualty == CasualtyType.Killed;
        var recordDef = isKillLog ? HistoryRecordDefOf.Death : HistoryRecordDefOf.Downed;
        var historyRecords = subject.HistoryRecords;
        var lastRecord = historyRecords.LastOrDefault();

        if (isKillLog && lastRecord?.def == HistoryRecordDefOf.Downed && lastRecord?.date == GenTicks.TicksAbs)
        {
            Log.Message($"[PawnHistory] Received death & downed transitions from {subject.NameShortColored} in the same tick. Skipping down transition..");
            historyRecords.Pop();
        }

        string desc;
        // combatLogText is null when:
        // - Log entry is not associated with any active battle. Non-combat dead needs to be handled manually (e.g. BloodLoss, ToxicBuildup...)
        if (combatLogText == null)
        {
            var hediffInt = subject.health.hediffSet.hediffs.LastOrDefault(h => h.def == culpritHediff && h.ageTicks == 0);
            hediffInt ??= subject.health.hediffSet.hediffs.LastOrDefault(h => h.ageTicks == 0 && h.def.isBad); // sometimes IF it does not find anything, use bruteforce
            var rootKeyword = isKillLog ? "killedEntry" : "downedEntry";
            desc = recordDef.Description(subject)
                .IncludePawnGrammar()
                .AddConstant("factionLeader", isLeader)
                .AddRule("HediffInPart", hediffInt?.LabelNounPretty())
                .AddConstant("hasReason", hediffInt != null)
                .Resolve(rootKeyword);
        }
        else
            desc = $"{combatLogText} {transitionText}";

        AddRecord(recordDef, subject, desc, [initiator, originalTarget]);

        if (isKillLog)
            CreatePovDeathRecord(subject, initiator, originalTarget, combatLogText, transitionText, desc);
    }

    private void CreatePovDeathRecord(Pawn deceased, Pawn initiator, Pawn originalTarget, string combatLogText, string transitionText, string deathDesc)
    {
        foreach (var relative in deceased.relations.PotentiallyRelatedPawns)
        {
            if (!ShouldRecord(relative))
                continue;

            var relationDef = relative.GetMostImportantRelation(deceased);
            if (relationDef == null || (relative.relations?.DirectRelationExists(PawnRelationDefOf.Bond, deceased) ?? false)) continue;

            var recordDef = HistoryRecordDefOf.RelativeDeath;
            var pov = recordDef.Description(relative)
                .AddRule("Relation", relationDef.GetGenderSpecificLabel(deceased))
                .AddRule("Subject", deceased)
                .Resolve("povRelative");
            var desc = CreatePovDescription(deceased, pov);

            AddRecord(recordDef, relative, desc, [deceased, initiator, originalTarget]);
        }

        if (!RelationHelper.TryGetBondedHumans(deceased, out var bondedHumans))
            return;

        foreach (var human in bondedHumans)
        {
            if (!ShouldRecord(human))
                continue;

            var recordDef = HistoryRecordDefOf.BondedAnimalDeath;
            var pov = recordDef.Description(human)
                .AddRule("AnimalKind", deceased.kindDef)
                .AddRule("Subject", deceased)
                .AddConstant("hasName", deceased.Name != null)
                .Resolve("povBondedAnimal");
            var desc = CreatePovDescription(deceased, pov);

            AddRecord(recordDef, human, desc, [deceased, initiator, originalTarget]);
        }

        return;

        string CreatePovDescription(Pawn deadPawn, string pov)
        {
            const StringComparison comparisonType = StringComparison.OrdinalIgnoreCase; // 'the elephant' is capitalized as the first word 
            var name = deadPawn.NameDef;
            // "A died" -> "C's brother, A, died"
            return combatLogText != null
                ? transitionText.ReplaceFirstMatch(name, pov, comparisonType) + " " + combatLogText
                : deathDesc.ReplaceFirstMatch(name, pov, comparisonType).ReplaceFirstMatch(",,", ","); // factionLeader==True + relativePov
        }
    }

    protected override void AddRecord(
        HistoryRecordDef def,
        Pawn pawn,
        TaggedString resolvedDesc,
        IEnumerable<Thing> concerns = null,
        RecordLocation location = null,
        int? tileId = null,
        Quest quest = null)
    {
        if (def == HistoryRecordDefOf.Death)
        {
            var lastRecord = pawn.HistoryRecords.LastOrDefault();
            if (lastRecord == null)
                return;
            if (lastRecord.def == HistoryRecordDefOf.Crushed || lastRecord.def == HistoryRecordDefOf.FriendlyTrapHit)
                location = lastRecord.location;
        }
        if (def == HistoryRecordDefOf.RelativeDeath)
        {
            var deathRelative = concerns.FirstOrDefault() as Pawn;
            var lastTwoRecords = deathRelative.HistoryRecords.TakeLast(2).ToList();
            var secondLastRecord = lastTwoRecords.FirstOrDefault();
            var lastRecord = lastTwoRecords.LastOrDefault();
            if (lastTwoRecords.Count == 2 && (secondLastRecord.def == HistoryRecordDefOf.Crushed || secondLastRecord.def == HistoryRecordDefOf.FriendlyTrapHit) && lastRecord.def == HistoryRecordDefOf.Death)
                location = lastRecord?.location;
        }

        base.AddRecord(def, pawn, resolvedDesc, concerns, location, tileId, quest);
    }

    [TestTag("Flaky")]
    public Action TestDeadInCombat(TestScenario scenario)
    {
        var friends = scenario.RaidFriendly().Point(700).RaidNeverFlee().Execute();
        var enemies = scenario.Incident(IncidentDefOf.RaidEnemy).Point(500).RaidNeverFlee().Execute();
        var pawns = scenario.Pawn(friends.Concat(enemies))
            .ThatMatches(ShouldRecord)
            .SetRandomRelations(5)
            .Execute();

        scenario.SpeedUp();

        Expect.ThatAny(pawns).Eventually().ToHaveHistoryRecordOf(HistoryRecordDefOf.Downed);
        Expect.ThatAny(pawns).Eventually().ToHaveHistoryRecordOf(HistoryRecordDefOf.Death);
        Expect.ThatAny(pawns).Eventually().ToHaveHistoryRecordOf(HistoryRecordDefOf.RelativeDeath);
        Expect.ThatAny(pawns).Eventually().ToHaveHistoryRecordOf(HistoryRecordDefOf.Kill);

        return () => scenario.SlowDown();
    }

    public void TestRelativeDead(TestScenario scenario)
    {
        var victim = scenario.Pawn().CreateSingle();
        var friend = scenario.Pawn().SetRelation(victim, PawnRelationDefOf.Lover).CreateSingle();
        HealthUtility.DamageUntilDead(victim);

        Expect.That(victim).ToHaveHistoryRecord(HistoryRecordDefOf.Death, "[PAWN] died because of [HediffInPart].");
        Expect.That(friend).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RelativeDeath,
            Description = "[PAWN]'s lover, [Subject], died because of [HediffInPart].",
            Concerns = [victim]
        });
    }

    public void TestBondedAnimalDead(TestScenario scenario)
    {
        var bondedAnimal = scenario.Pawn().Animal(Extra.PawnKindDefOf.Husky).CreateSingle();
        var human = scenario.Pawn().Colonist().SetRelation(bondedAnimal, PawnRelationDefOf.Bond).CreateSingle();
        
        HealthUtility.DamageUntilDead(bondedAnimal);

        Expect.That(bondedAnimal).ToHaveHistoryRecord(HistoryRecordDefOf.Death, "The husky died because of [HediffInPart].");
        Expect.That(human).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BondedAnimalDeath,
            Description = "[PAWN]'s bonded husky died because of [HediffInPart].",
            Concerns = [bondedAnimal]
        });
    }

    public void TestBondedAnimalDead2(TestScenario scenario)
    {
        var bondedAnimal = scenario.Pawn().Animal(PawnKindDefOf.Alphabeaver).Do(p => p.Name = new NameSingle("Steve")).CreateSingle();
        var human = scenario.Pawn().Colonist().SetRelation(bondedAnimal, PawnRelationDefOf.Bond) .CreateSingle();
        
        HealthUtility.DamageUntilDead(bondedAnimal);

        Expect.That(bondedAnimal).ToHaveHistoryRecord(HistoryRecordDefOf.Death, "Steve died because of [HediffInPart].");
        Expect.That(human).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.BondedAnimalDeath,
            Description = "[PAWN]'s bonded alphabeaver, Steve, died because of [HediffInPart].",
            Concerns = [bondedAnimal]
        });
    }

    public void TestLeaderDead(TestScenario scenario)
    {
        var leader = scenario.Pawn().FactionLeader(Faction.OfPirates).CreateSingle();
        var spouse = scenario.Pawn().SetRelation(leader, PawnRelationDefOf.ExLover).CreateSingle();
        HealthUtility.DamageUntilDead(leader);

        Expect.That(leader).ToHaveHistoryRecord(HistoryRecordDefOf.Death, "[PAWN], a faction leader of [PAWN_factionName], died because of [HediffInPart].");
        Expect.That(spouse).ToHaveHistoryRecord(new ExpectedHistoryRecord
        {
            Def = HistoryRecordDefOf.RelativeDeath,
            Description = "[PAWN]'s ex-lover, [Subject], a faction leader of [PAWN_factionName], died because of [HediffInPart].",
            Concerns = [leader]
        });
    }

    public void TestDead(TestScenario scenario)
    {
        var pawn = scenario.Pawn().CreateSingle();
        HealthUtility.DamageUntilDead(pawn);

        var pawn2 = scenario.Pawn().FullHeal().CreateSingle();
        pawn2.Kill(null);

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.Death, "[PAWN] died because of [HediffInPart].");
        Expect.That(pawn2).ToHaveHistoryRecord(HistoryRecordDefOf.Death, "[PAWN] died.", exactMatch: true);
    }

    public void TestDowned(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Enemy().CreateSingle();
        HealthUtility.DamageUntilDowned(pawn);

        var pawn2 = scenario.Pawn().Enemy().FullHeal().CreateSingle();
        pawn2.MakeDowned();

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.Downed, "[PAWN] was incapacitated due to [HediffInPart].");
        Expect.That(pawn2).ToHaveHistoryRecord(HistoryRecordDefOf.Downed, "[PAWN] was incapacitated.", exactMatch: true);
    }
}
