using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class DeathrestOrComaRecorder : RecorderBase<DeathrestOrComaEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<DeathrestOrComaEvent>(CreateRecord);
    }

    public override void CreateRecord(DeathrestOrComaEvent e)
    {
        var pawn = e.Pawn;
        if (!ShouldRecord(pawn))
            return;

        if (e.Reason != DeathrestStartReason.LethalDamage)
            return;

        var recordDef = HistoryRecordDefOf.DeathrestOrComa;

        AddRecord(recordDef, pawn, () =>
        {
            var desc = recordDef.Description(pawn)
                .AddConstant("reason", e.Reason)
                .AddConstant("isDeathRest", e.IsDeathRest)
                .Resolve();
            var logEntry = Find.BattleLog.Battles.Where(b => b.Concerns(pawn)).SelectMany(b => b.Entries).FirstOrDefault(entry => IsRelatedCombatEntry(entry, pawn)) as LogEntry_DamageResult;
            var concerns = logEntry?.GetConcerns() ?? [];
            var combatLog = logEntry?.ToGameStringFromPOV(null); // at this point, DamageWorker.AssociateWithLog() was finished.
            
            if (!string.IsNullOrEmpty(combatLog))
                desc = $"{combatLog} {desc}";
            
            return new HistoryRecordWriteRequest(recordDef, pawn, desc, concerns);
        });
    }

    private static bool IsRelatedCombatEntry(LogEntry entry, Pawn pawn)
    {
        if (GenTicks.TicksAbs - entry.Tick > 1)
            return false;

        if (entry is not LogEntry_DamageResult drEntry)
            return false;

        if (!drEntry.Concerns(pawn))
            return false;
        
        var damagedPartsDestroyed = Accessor.LogEntry_DamageResult.DamagedPartsDestroyed(drEntry) ?? [];
        if (damagedPartsDestroyed.Any(p => p))
            return false; // avoid duplicate record with BodyPartDestroyed

        return true;
    }

    [RequiresBiotech]
    public void Test(TestScenario scenario)
    {
        var deathrestPawn = scenario.Pawn()
            .Colonist()
            .ResetSkillLevel(2, -1000)
            .AddGenes([GeneDefOf.Deathless, Extra.GeneDefOf.Deathrest])
            .TakeDamage(999f, BodyPartDefOf.Heart)
            .CreateSingle();
        var comaPawn = scenario.Pawn()
            .Colonist()
            .ResetSkillLevel(2, -1000)
            .AddGenes([GeneDefOf.Deathless])
            .TakeDamage(999f, BodyPartDefOf.Heart)
            .CreateSingle();

        Expect.That(deathrestPawn).ToHaveHistoryRecord(HistoryRecordDefOf.DeathrestOrComa, "[PAWN] entered deathrest after suffering lethal damage.");
        Expect.That(deathrestPawn).ToHaveTheLastHistoryRecordsOf([HistoryRecordDefOf.BodyPartDestroyed, HistoryRecordDefOf.DeathrestOrComa, HistoryRecordDefOf.SkillLeveledDown]);
        Expect.That(deathrestPawn.HistoryRecords.Select(r => r.def)).Not().Contain(HistoryRecordDefOf.Downed);
        Expect.That(comaPawn).ToHaveHistoryRecord(HistoryRecordDefOf.DeathrestOrComa, "[PAWN] entered a regenerative coma after suffering lethal damage.");
        Expect.That(comaPawn).ToHaveTheLastHistoryRecordsOf([HistoryRecordDefOf.BodyPartDestroyed, HistoryRecordDefOf.DeathrestOrComa, HistoryRecordDefOf.SkillLeveledDown]);
    }

    [RequiresBiotech]
    public void TestCombat(TestScenario scenario)
    {
        var friends = scenario.RaidFriendly().Point(1500).RaidNeverFlee().Execute();
        var enemies = scenario.Incident(IncidentDefOf.RaidEnemy).Point(1200).RaidNeverFlee().Execute();
        var pawns = scenario.Pawn(friends.Concat(enemies))
            .ThatMatches(ShouldRecord) // exclude combat animals
            .Armed()
            .AddHediff(Extra.HediffDefOf.GoJuiceHigh)
            .AddGenes([GeneDefOf.Deathless, Extra.GeneDefOf.Deathrest])
            .Execute();

        Find.Storyteller.difficulty.enemyDeathOnDownedChanceFactor = 0f;
        Expect.ThatAny(pawns).Eventually(5000).ToHaveHistoryRecord(HistoryRecordDefOf.DeathrestOrComa, "[CombatLog]. [PAWN] entered [coma].");
        
        scenario.SpeedUp();
    }
}
