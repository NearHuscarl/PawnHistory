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
    public record KillInput(Pawn Subject, Pawn Initiator, string combatLogText, string transitionText, Pawn originalTargetPawn);
    public record KilledOrDownedInput(Pawn Subject, Pawn Initiator, CasualtyType Casualty, HediffDef CulpritHediff, string combatLogText, string transitionText, Pawn originalTargetPawn, bool isLeader);

    public override void Register()
    {
        GameEventBus.Subscribe<CasualtyLogAddedEvent>(e =>
        {
            var isLeader = e.Subject.IsFactionLeader(); // cache leader before it is reassigned next tick

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
        var historyRecords = subject.GetHistoryRecords();
        var lastRecord = historyRecords.LastOrDefault();

        if (isKillLog && lastRecord?.def == HistoryRecordDefOf.Downed && lastRecord.date == GenTicks.TicksAbs)
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
            RecordRelativeDeath(subject, initiator, originalTarget, combatLogText, transitionText, desc);
    }

    private void RecordRelativeDeath(Pawn deceased, Pawn initiator, Pawn originalTarget, string combatLogText, string transitionText, string deathDesc)
    {
        var recordDef = HistoryRecordDefOf.RelativeDeath;
        var deceasedName = deceased.NameDef();

        foreach (var relative in deceased.relations.PotentiallyRelatedPawns)
        {
            if (!ShouldRecord(relative))
                continue;

            var relationDef = relative.GetMostImportantRelation(deceased);
            if (relationDef == null) continue;

            var relativePov = recordDef.Description(relative)
                .AddRule("Relation", relationDef.GetGenderSpecificLabel(deceased))
                .AddRule("Subject", deceased)
                .Resolve("relativePov");

            // "A died" -> "C's brother, A, died"
            var desc = combatLogText != null
                ? transitionText.ReplaceFirstMatch(deceasedName, relativePov) + " " + combatLogText
                : deathDesc.ReplaceFirstMatch(deceasedName, relativePov).ReplaceFirstMatch(",,", ","); // factionLeader==True + relativePov

            AddRecord(recordDef, relative, desc, [deceased, initiator, originalTarget]);
        }
    }

    protected override void AddRecord(HistoryRecordDef def, Pawn pawn, TaggedString resolvedDesc, IEnumerable<Thing> concerns = null, RecordLocation location = null)
    {
        if (def == HistoryRecordDefOf.Death)
        {
            var lastRecord = pawn.GetHistoryRecords().LastOrDefault();
            if (lastRecord?.def == HistoryRecordDefOf.Crushed || lastRecord?.def == HistoryRecordDefOf.FriendlyTrapHit)
                location = lastRecord.location;
        }
        if (def == HistoryRecordDefOf.RelativeDeath)
        {
            var deathRelative = concerns.FirstOrDefault() as Pawn;
            var lastTwoRecords = deathRelative.GetHistoryRecords().TakeLast(2).ToList();
            var secondLastRecord = lastTwoRecords.FirstOrDefault();
            var lastRecord = lastTwoRecords.LastOrDefault();
            if (lastTwoRecords.Count == 2 && (secondLastRecord.def == HistoryRecordDefOf.Crushed || secondLastRecord.def == HistoryRecordDefOf.FriendlyTrapHit) && lastRecord.def == HistoryRecordDefOf.Death)
                location = lastRecord?.location;
        }

        base.AddRecord(def, pawn, resolvedDesc, concerns, location);
    }

    public Action TestDeadInCombat(TestScenario scenario)
    {
        var friends = scenario.RaidFriendly()
            .Point(700)
            .Execute();

        var enemies = scenario.Incident(IncidentDefOf.RaidEnemy)
            .Point(500)
            .Execute();

        scenario.Pawn(friends.Concat(enemies))
            .ThatMatches(ShouldRecord)
            .DiesOnNextHit()
            .SetRandomRelations(5)
            .Execute();

        scenario.SpeedUp();

        Expect.AnyPawnOnMap().Eventually().ToHaveHistoryRecordOf(HistoryRecordDefOf.Downed);
        Expect.AnyPawnOnMap().Eventually().ToHaveHistoryRecordOf(HistoryRecordDefOf.Death);
        Expect.AnyPawnOnMap().Eventually().ToHaveHistoryRecordOf(HistoryRecordDefOf.RelativeDeath);
        Expect.AnyPawnOnMap().Eventually().ToHaveHistoryRecordOf(HistoryRecordDefOf.Kill);

        return () => scenario.SlowDown();
    }

    public void TestLeaderDead(TestScenario scenario)
    {
        var leader = scenario.Pawn().FactionLeader(Faction.OfPirates).CreateSingle();
        var spouse = scenario.Pawn().SetRelation(leader, PawnRelationDefOf.Spouse).CreateSingle();
        HealthUtility.DamageUntilDead(leader);

        Expect.That(leader).ToHaveHistoryRecord("[PAWN], a faction leader of [PAWN_factionName], died because of [HediffInPart].");
        Expect.That(spouse).ToHaveHistoryRecord("[PAWN]'s [Relation], [Subject], a faction leader of [PAWN_factionName], died because of [HediffInPart].");
    }

    public void TestDead(TestScenario scenario)
    {
        var pawn = scenario.Pawn().CreateSingle();
        HealthUtility.DamageUntilDead(pawn);

        var pawn2 = scenario.Pawn().CreateSingle();
        pawn2.Kill(null);

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] died because of [HediffInPart].");
        Expect.That(pawn2).ToHaveHistoryRecord("[PAWN] died.", exactMatch: true);
    }

    public void TestDowned(TestScenario scenario)
    {
        var pawn = scenario.Pawn().Enemy().CreateSingle();
        HealthUtility.DamageUntilDowned(pawn);

        var pawn2 = scenario.Pawn().Enemy().CreateSingle();
        pawn2.MakeDowned();

        Expect.That(pawn).ToHaveHistoryRecord("[PAWN] was incapacitated due to [HediffInPart].");
        Expect.That(pawn2).ToHaveHistoryRecord("[PAWN] was incapacitated.", exactMatch: true);
    }
}
