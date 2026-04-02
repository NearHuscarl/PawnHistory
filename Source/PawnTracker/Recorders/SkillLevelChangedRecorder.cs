using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

internal class SkillLevelChangedRecorder : RecorderBase
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
        GameEventBus.Subscribe<SkillLevelChangedEvent>(e =>
        {
            if (!ShouldRecord(e.Pawn))
                return;

            if (e.NewLevel > e.OldLevel)
                HandleLeveledUpEvent(e);
            else
                HandleLeveledDownEvent(e);
        });
    }

    private void HandleLeveledUpEvent(SkillLevelChangedEvent e)
    {
        var historyComp = CompHistoryManager.GetComp(e.Pawn);
        var recordDef = HistoryRecordDefOf.SkillLeveledUp;
        var builder = recordDef.Description(e.Pawn)
            .AddRule("NewLevel", e.NewLevel)
            .AddRule("Skill", e.Def.skillLabel);

        if (SkillRecords.TryGetValue(e.Def, out var recordsToUpdate))
        {
            recordsToUpdate = recordsToUpdate.Where(r => r != null).ToArray();
            var dominant = historyComp.DominantDelta(e.Def, recordsToUpdate);
            if (dominant is { } d && d.Delta > 0)
                builder.AddRule("RecordCount", e.Pawn.records.GetAsInt(d.Def))
                    .AddConstant("record", d.Def.defName);
            
            historyComp.UpdateSnapshot(recordsToUpdate);
        }

        AddRecord(recordDef, e.Pawn, builder.Resolve());
    }

    private void HandleLeveledDownEvent(SkillLevelChangedEvent e)
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
        var pawns = scenario.Pawn()
            .Colonist()
            .SetSkillLevel(SkillDefOf.Shooting, 5)
            .Execute();

        scenario.Pawn(pawns)
            .Do(p => p.records.AddTo(RecordDefOf.Kills, 20))
            .Do(p => p.records.AddTo(RecordDefOf.KillsAnimals, 15))
            .Do(p => p.records.AddTo(RecordDefOf.KillsMechanoids, 15))
            .Learn(SkillDefOf.Shooting, 20_000)
            .Execute();

        scenario.Pawn(pawns)
            .Do(p => p.records.AddTo(RecordDefOf.Kills, 20))
            .Do(p => p.records.AddTo(RecordDefOf.KillsAnimals, 15))
            .Do(p => p.records.AddTo(RecordDefOf.KillsMechanoids, 15))
            .Learn(SkillDefOf.Shooting, 30_000)
            .Execute();

        scenario.Pawn(pawns)
            .Do(p => p.records.AddTo(RecordDefOf.Kills, 20))
            .Do(p => p.records.AddTo(RecordDefOf.KillsAnimals, 15))
            .Do(p => p.records.AddTo(RecordDefOf.KillsMechanoids, 15))
            .Learn(SkillDefOf.Shooting, 40_000)
            .Execute();

        scenario.Pawn(pawns)
            .Do(p => p.records.AddTo(RecordDefOf.Kills, 20))
            .Do(p => p.records.AddTo(RecordDefOf.KillsAnimals, 15))
            .Do(p => p.records.AddTo(RecordDefOf.KillsMechanoids, 15))
            .Learn(SkillDefOf.Shooting, 70_000)
            .Execute();

        // should show:
        // Alvarez reached level 8 in shooting after killing 20 creatures.
        // Alvarez reached level 10 in shooting after hunting 30 animals.
        // Alvarez reached level 12 in shooting after killing 45 mechanoids.
        // Alvarez reached level 13 in shooting after killing 80 creatures.
        scenario.OpenHistoryRecordTab(pawns[0]);
    }

    public void TestDown(TestScenario scenario)
    {
        var pawns = scenario.Pawn()
            .Colonist()
            .SetSkillLevel(SkillDefOf.Shooting, 20)
            .Execute();

        scenario.Pawn(pawns)
            .Learn(SkillDefOf.Shooting, -200_000)
            .Execute();

        scenario.OpenHistoryRecordTab(pawns[0]);
    }
}
