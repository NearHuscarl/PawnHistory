using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class CasualtyContext(Battle battle, BattleLogEntry_StateTransition transitionEntry, Pawn subject)
{
    private LogEntry_DamageResult AssociatedDamageEntry
        => field ??= battle.Entries.FirstOrDefault(l => l.Tick == transitionEntry.Tick && l is LogEntry_DamageResult && l.Concerns(subject)) as LogEntry_DamageResult;

    public Pawn OriginalTargetPawn
        => field ??= AssociatedDamageEntry is BattleLogEntry_RangedImpact r ? Accessor.BattleLogEntry_RangedImpact.OriginalTargetPawn(r) : null;

    public string CombatLogText(Pawn pov) => AssociatedDamageEntry?.ToGameStringFromPOV(pov);
    public string TransitionText(Pawn pov) => transitionEntry.ToGameStringFromPOV(pov);
}

public class CasualtyRecorder : RecorderBase<CasualtyRecorder.KillInput>, IRecord<CasualtyRecorder.KilledOrDownedInput>
{
    // TODO: add instigator from combat log in case transition log misses it. 
    public record KillInput(Pawn Subject, Pawn Initiator, CasualtyContext Context);
    public record KilledOrDownedInput(Pawn Subject, Pawn Initiator, CasualtyType Casualty, HediffDef CulpritHediff, CasualtyContext Context, bool IsLeader);

    private static readonly HashSet<HediffDef> CulpritHediffBlacklist = new List<HediffDef>
    {
        HediffDefOf.Anesthetic,
        HediffDefOf.RegenerationComa, // handled in DeathrestOrComaRecorder
    }.Where(h => h != null).ToHashSet();

    public override void Register()
    {
        GameEventBus.Subscribe<CasualtyLogAddedEvent>(e =>
        {
            var isLeader = e.Subject.IsFactionLeader();
            var context = new CasualtyContext(e.Battle, e.TransitionEntry, e.Subject);

            if (e.Casualty == CasualtyType.Killed)
                CreateRecord(new KillInput(e.Subject, e.Initiator, context));

            if (!CulpritHediffBlacklist.Contains(e.CulpritHediff))
                CreateRecord(new KilledOrDownedInput(e.Subject, e.Initiator, e.Casualty, e.CulpritHediff, context, isLeader));
        });
    }

    public override void CreateRecord(KillInput input)
    {
        var (subject, initiator, context) = input;
        if (!ShouldRecord(initiator))
            return;

        // Use callback to delay until after DamageWorker.AssociateWithLog() runs, since it populates the body part data required to retrieve the exact in-game combat log.
        AddRecord(HistoryRecordDefOf.Kill, initiator, () =>
        {
            var combatLogText = context.CombatLogText(initiator);
            var transitionText = context.TransitionText(initiator);
            var desc = $"{combatLogText} {transitionText}";
            return new HistoryRecordWriteRequest(HistoryRecordDefOf.Kill, initiator, desc, [subject, context.OriginalTargetPawn]);
        });
    }

    public void CreateRecord(KilledOrDownedInput input)
    {
        var (subject, initiator, casualty, culpritHediff, context, isLeader) = input;
        if (!ShouldRecord(subject))
            return;

        var isKillLog = casualty == CasualtyType.Killed;
        var recordDef = isKillLog ? HistoryRecordDefOf.Death : HistoryRecordDefOf.Downed;

        AddRecord(recordDef, subject, () =>
        {
            var combatLogText = context.CombatLogText(subject);
            var transitionText = context.TransitionText(subject);
            // combatLogText is null when:
            // - Log entry is not associated with any active battle. Non-combat dead needs to be handled manually (e.g. BloodLoss, ToxicBuildup...)
            var desc = combatLogText != null
                ? $"{combatLogText} {transitionText}"
                : GetFallbackDeathDescription(input);

            RecordLocation location = null;
            if (isKillLog)
            {
                var lastRecord = subject.HistoryRecords.LastOrDefault();
                if (lastRecord != null && IsDeathLocationSource(lastRecord.def))
                    location = lastRecord.location;

                if (lastRecord?.def == HistoryRecordDefOf.Downed && GenTicks.TicksAbs - lastRecord?.date <= 1)
                {
                    Log.Message($"[PawnHistory] Received death & downed transitions from {subject.NameShortColored} in the same tick. Skipping down transition..");
                    subject.HistoryRecords.Pop();
                }
            }

            return new HistoryRecordWriteRequest(recordDef, subject, desc, [initiator, context.OriginalTargetPawn], location);
        });

        if (isKillLog)
            CreatePovDeathRecord(input);
    }

    private void CreatePovDeathRecord(KilledOrDownedInput input)
    {
        var (deceased, initiator, _, _, context, _) = input;
        var originalTarget = context.OriginalTargetPawn;
        foreach (var relative in deceased.relations.PotentiallyRelatedPawns)
        {
            if (!ShouldRecord(relative))
                continue;

            var relationDef = relative.GetMostImportantRelation(deceased);
            if (relationDef == null || (relative.relations?.DirectRelationExists(PawnRelationDefOf.Bond, deceased) ?? false)) continue;

            AddRecord(HistoryRecordDefOf.RelativeDeath, relative, () =>
            {
                var recordDef = HistoryRecordDefOf.RelativeDeath;
                var pov = recordDef.Description(relative)
                    .AddRule("Relation", relationDef.GetGenderSpecificLabel(deceased))
                    .AddRule("Subject", deceased)
                    .Resolve("povRelative");
                var desc = CreatePovDescription(pov);

                RecordLocation location = null;
                var lastRecord = deceased.HistoryRecords.LastOrDefault();
                if (lastRecord?.def == HistoryRecordDefOf.Death)
                    location = lastRecord.location;

                return new HistoryRecordWriteRequest(recordDef, relative, desc, [deceased, initiator, originalTarget], location);
            });
        }

        if (!RelationHelper.TryGetBondedHumans(deceased, out var bondedHumans))
            return;

        foreach (var human in bondedHumans)
        {
            if (!ShouldRecord(human))
                continue;

            var recordDef = HistoryRecordDefOf.BondedAnimalDeath;
            AddRecord(recordDef, human, () =>
            {
                var pov = recordDef.Description(human)
                    .AddRule("AnimalKind", deceased.kindDef)
                    .AddRule("Subject", deceased)
                    .AddConstant("hasName", deceased.Name != null)
                    .Resolve("povBondedAnimal");
                var desc = CreatePovDescription(pov);

                return new HistoryRecordWriteRequest(recordDef, human, desc, [deceased, initiator, originalTarget]);
            });
        }

        return;

        string CreatePovDescription(string pov)
        {
            var combatLogText = context.CombatLogText(deceased);
            var transitionText = context.TransitionText(deceased);
            var deathDesc = GetFallbackDeathDescription(input);
            const StringComparison comparisonType = StringComparison.OrdinalIgnoreCase; // 'the elephant' is capitalized as the first word 
            var name = deceased.NameDef;
            // "A died" -> "C's brother, A, died"
            return combatLogText != null
                ? transitionText.ReplaceFirstMatch(name, pov, comparisonType) + " " + combatLogText
                : deathDesc.ReplaceFirstMatch(name, pov, comparisonType).ReplaceFirstMatch(",,", ","); // factionLeader==True + relativePov
        }
    }

    private static string GetFallbackDeathDescription(KilledOrDownedInput input)
    {
        var (subject, _, casualty, culpritHediff, _, isLeader) = input;
        var isKillLog = casualty == CasualtyType.Killed;
        var recordDef = isKillLog ? HistoryRecordDefOf.Death : HistoryRecordDefOf.Downed;
        var hediffInt = subject.health.hediffSet.hediffs.LastOrDefault(h => h.def == culpritHediff) ?? subject.health.hediffSet.hediffs.LastOrDefault(h => h.def.isBad && h.ageTicks <= 1);
        var rootKeyword = isKillLog ? "killedEntry" : "downedEntry";
        return recordDef.Description(subject)
            .IncludePawnGrammar()
            .AddConstant("factionLeader", isLeader)
            .AddRule("HediffInPart", hediffInt?.LabelNounPretty())
            .AddConstant("hasReason", hediffInt != null)
            .Resolve(rootKeyword);
    }

    private static bool IsDeathLocationSource(HistoryRecordDef def) =>
        def == HistoryRecordDefOf.Crushed ||
        def == HistoryRecordDefOf.FriendlyTrapHit;

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
