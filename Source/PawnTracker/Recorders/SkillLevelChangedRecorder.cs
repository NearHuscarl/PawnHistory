using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class SkillLevelChangedRecorder : RecorderBase<SkillLevelChangedEvent>
{
    private static readonly List<RecordDef> CombatRecords = [
        RecordDefOf.Kills,
        RecordDefOf.KillsHumanlikes,
        RecordDefOf.KillsAnimals,
        RecordDefOf.KillsMechanoids,
        RecordDefOf.KillsEntities,
        RecordDefOf.PawnsDowned,
        RecordDefOf.PawnsDownedHumanlikes,
        RecordDefOf.PawnsDownedAnimals,
        RecordDefOf.PawnsDownedMechanoids,
        RecordDefOf.PawnsDownedEntities,
    ];

    private static readonly Dictionary<SkillDef, RecordDef[]> SkillRecords = new()
    {
        [SkillDefOf.Shooting] = CombatRecords.Concat([RecordDefOf.ShotsFired, RecordDefOf.Headshots]).ToArray(),
        [SkillDefOf.Melee] = CombatRecords.ToArray(),
        [SkillDefOf.Medicine] = [RecordDefOf.OperationsPerformed, RecordDefOf.TimesTendedOther],
        [SkillDefOf.Crafting] = [RecordDefOf.ThingsCrafted],
        [SkillDefOf.Mining] = [RecordDefOf.CellsMined],
        [SkillDefOf.Plants] = [RecordDefOf.PlantsSown, RecordDefOf.PlantsHarvested],
        [SkillDefOf.Cooking] = [RecordDefOf.MealsCooked],
        [SkillDefOf.Construction] = [RecordDefOf.ThingsConstructed, RecordDefOf.ThingsRepaired, RecordDefOf.ThingsDeconstructed],
        [SkillDefOf.Animals] = [RecordDefOf.AnimalsTamed],
        [SkillDefOf.Social] = [RecordDefOf.PrisonersRecruited, RecordDefOf.PrisonersChatted],
        [SkillDefOf.Intellectual] = [RecordDefOf.ResearchPointsResearched],
    };

    public override void Register()
    {
        GameEventBus.Subscribe<SkillLevelChangedEvent>(CreateRecord);
    }

    public override void CreateRecord(SkillLevelChangedEvent e)
    {
        if (!ShouldRecord(e.Pawn))
            return;

        if (e.NewLevel > e.OldLevel)
            RecordLeveledUp(e);
        else
            RecordLeveledDown(e);
    }

    private void RecordLeveledUp(SkillLevelChangedEvent e)
    {
        var historyComp = CompHistoryManager.GetComp(e.Pawn);
        var recordDef = HistoryRecordDefOf.SkillLeveledUp;
        var builder = recordDef.Description(e.Pawn)
            .AddRule("NewLevel", e.NewLevel)
            .AddRule("Skill", e.Def.skillLabel);

        if (SkillRecords.TryGetValue(e.Def, out var recordsToUpdate))
        {
            recordsToUpdate = recordsToUpdate.Where(r => r != null).ToArray();
            var skillLevelChangedState = historyComp.SkillLevelChangedState;
            var dominant = skillLevelChangedState.DominantDelta(e.Pawn, e.Def, recordsToUpdate);
            if (dominant is { Delta: > 0 })
            {
                builder.AddRule("RecordCount", e.Pawn.records.GetAsInt(dominant.Def))
                    .AddConstant("record", dominant.Def.defName);
            }
            
            skillLevelChangedState.UpdateSnapshot(e.Pawn, recordsToUpdate);
        }

        AddRecord(recordDef, e.Pawn, builder.Resolve());
    }

    private void RecordLeveledDown(SkillLevelChangedEvent e)
    {
        var recordDef = HistoryRecordDefOf.SkillLeveledDown;
        var desc = recordDef.Description(e.Pawn)
            .AddRule("NewLevel", e.NewLevel)
            .AddRule("Skill", e.Def.skillLabel)
            .Resolve();
        AddRecord(recordDef, e.Pawn, desc);
    }

    public void TestUp(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .ResetSkillLevel(SkillDefOf.Shooting, 1)
            .ResetRecords()
            .CreateSingle();
        CompHistoryManager.GetComp(pawn).ClearAll(); // remove record snapshots

        scenario.Pawn(pawn)
            .Do(p => p.records.AddTo(RecordDefOf.Kills, 20))
            .Do(p => p.records.AddTo(RecordDefOf.KillsAnimals, 15))
            .Do(p => p.records.AddTo(RecordDefOf.KillsMechanoids, 15))
            .Learn(SkillDefOf.Shooting, 20_000)
            .Execute();

        scenario.Pawn(pawn)
            .Do(p => p.records.AddTo(RecordDefOf.Kills, 20))
            .Do(p => p.records.AddTo(RecordDefOf.KillsAnimals, 15))
            .Do(p => p.records.AddTo(RecordDefOf.KillsMechanoids, 15))
            .Learn(SkillDefOf.Shooting, 30_000)
            .Execute();

        scenario.Pawn(pawn)
            .Do(p => p.records.AddTo(RecordDefOf.Kills, 20))
            .Do(p => p.records.AddTo(RecordDefOf.KillsAnimals, 15))
            .Do(p => p.records.AddTo(RecordDefOf.KillsMechanoids, 15))
            .Learn(SkillDefOf.Shooting, 40_000)
            .Execute();

        scenario.Pawn(pawn)
            .Do(p => p.records.AddTo(RecordDefOf.Kills, 20))
            .Do(p => p.records.AddTo(RecordDefOf.KillsAnimals, 15))
            .Do(p => p.records.AddTo(RecordDefOf.KillsMechanoids, 15))
            .Learn(SkillDefOf.Shooting, 70_000)
            .Execute();

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.SkillLeveledUp, "[PAWN] reached level [NewLevel] in shooting after killing 20 creatures.", index: -4);
        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.SkillLeveledUp, "[PAWN] reached level [NewLevel] in shooting after hunting 30 animals.", index: -3);
        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.SkillLeveledUp, "[PAWN] reached level [NewLevel] in shooting after killing 45 mechanoids.", index: -2);
        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.SkillLeveledUp, "[PAWN] reached level [NewLevel] in shooting after killing 80 creatures.", index: -1);

        scenario.OpenHistoryRecordTab(pawn);
    }

    public void TestDown(TestScenario scenario)
    {
        var pawn = scenario.Pawn()
            .Colonist()
            .ResetSkillLevel(SkillDefOf.Shooting, 20)
            .CreateSingle();

        scenario.Pawn(pawn)
            .Learn(SkillDefOf.Shooting, -200_000)
            .Execute();

        Expect.That(pawn).ToHaveHistoryRecord(HistoryRecordDefOf.SkillLeveledDown, "[PAWN] dropped to level [NewLevel] in shooting.");
        scenario.OpenHistoryRecordTab(pawn);
    }
}
