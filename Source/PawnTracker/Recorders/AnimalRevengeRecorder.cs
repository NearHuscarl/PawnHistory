using PawnHistory.Source.DebugTools;
using PawnHistory.Source.PawnTracker.Events;
using PawnHistory.Source.PawnTracker.Test;
using RimWorld;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Recorders;

public class AnimalRevengeRecorder : RecorderBase<AnimalRevengeEvent>
{
    public override void Register()
    {
        GameEventBus.Subscribe<AnimalRevengeEvent>(CreateRecord);
    }

    public override void CreateRecord(AnimalRevengeEvent e)
    {
        if (!ShouldRecord(e.Instigator))
            return;

        var recordDef = HistoryRecordDefOf.AnimalRevenge;
        var desc = recordDef.Description(e.Instigator)
            .AddRule("MadAnimal", e.Animals.First())
            .WithOthers(e.Animals)
            .AddConstant("reason", e.Reason)
            .AddConstant("packRevenge", e.Animals.Count > 1)
            .Resolve();

        AddRecord(recordDef, e.Instigator, desc, e.Animals);
    }

    public void Test(TestScenario scenario)
    {
        var animals = scenario.Pawn(5).Animal(PawnKindDefOf.Muffalo).Execute().ToList();
        var instigator = scenario.Pawn().Colonist().CreateSingle();

        var oldValue = Find.Storyteller.difficulty.allowBigThreats;
        Find.Storyteller.difficulty.allowBigThreats = false;

        Accessor.Pawn_MindState.StartManhunterBecauseOfPawnAction(animals[0].mindState, instigator, "AnimalManhunterFromTaming", false);
        Accessor.Pawn_MindState.StartManhunterBecauseOfPawnAction(animals[1].mindState, instigator, "AnimalManhunterFromDamage", true);

        Expect.That(instigator).ToHaveHistoryRecord("The muffalo went manhunter after [PAWN] failed to tame it.", -2);
        Expect.That(instigator).ToHaveHistoryRecord("The muffalo went manhunter after being harmed by [PAWN].", -1);
        Find.Storyteller.difficulty.allowBigThreats = oldValue;
    }

    public void TestPackRevenge(TestScenario scenario)
    {
        var animals = scenario.Pawn(5).Animal(PawnKindDefOf.Muffalo).Execute().ToList();
        var instigator = scenario.Pawn().Colonist().CreateSingle();

        var oldValue = Find.Storyteller.difficulty.allowBigThreats;
        Find.Storyteller.difficulty.allowBigThreats = true;
        NearDebugSettings.ForceManhunterChance = true;

        Accessor.Pawn_MindState.StartManhunterBecauseOfPawnAction(animals[0].mindState, instigator, "AnimalManhunterFromDamage", true);

        Expect.That(instigator).ToHaveHistoryRecord("The muffalo went manhunter after being harmed by [PAWN], [Count] others nearby also became enraged!", -1);
        Find.Storyteller.difficulty.allowBigThreats = oldValue;
        NearDebugSettings.ForceManhunterChance = false;
    }
}
