using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class LetterActionGrowthMoment : LetterAction<ChoiceLetter_GrowthMoment>
{
    private IEnumerable<int> passionIndexes;
    private int? traitIndex;
    private bool ChooseNoTrait => traitIndex == null; 
    
    public override ChoiceLetter_GrowthMoment Execute()
    {
        base.Execute();
        PopulateGrowthChoices();

        var selectedPassions = ResolvePassions();
        var selectedTrait = ResolveTrait();

        Letter.MakeChoices(selectedPassions, selectedTrait);
        return Letter;
    }

    public LetterActionGrowthMoment TraitIndex(int? traitIndex2 = null)
    {
        this.traitIndex = traitIndex2;
        return this;
    }

    public LetterActionGrowthMoment PassionIndices(IEnumerable<int> passionIndexes2 = null)
    {
        this.passionIndexes = passionIndexes2;
        return this;
    }

    private void PopulateGrowthChoices()
    {
        var anyPawn = Find.CurrentMap.mapPawns.AllHumanlikeSpawned.First();
        Letter.passionChoices = DefDatabase<SkillDef>.AllDefsListForReading.InRandomOrder().Take(6).ToList();
        Letter.traitChoices = PawnGenerator.GenerateTraitsFor(anyPawn, 6, growthMomentTrait: true);
        Letter.noTraitOptionShown = true;
    }

    private List<SkillDef> ResolvePassions()
    {
        var indexes = (passionIndexes ?? []).ToList();
        var passions = Letter.passionChoices ?? [];

        if (indexes.Count != Letter.passionGainsCount)
            throw new InvalidOperationException($"Expected {Letter.passionGainsCount} passion selections but got {indexes.Count}.");

        return indexes.Select(index => passions[index]).ToList();
    }

    private Trait ResolveTrait()
    {
        var traits = Letter.traitChoices;

        if (ChooseNoTrait)
            return ChoiceLetter_GrowthMoment.NoTrait;

        return traits[traitIndex!.Value];
    }
}