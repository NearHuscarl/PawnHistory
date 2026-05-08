using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class LetterActionBabyToChild : LetterActionSimple<ChoiceLetter_BabyToChild>
{
    public LetterActionBabyToChild PickColonist()
    {
        SetChoice(LetterChoiceKind.Option1);
        return this;
    }

    public LetterActionBabyToChild PickSlave()
    {
        SetChoice(LetterChoiceKind.Option2);
        return this;
    }

    protected override DiaOption ResolveChoice(ChoiceLetter_BabyToChild letter)
    {
        var choices = letter.Choices.ToList();
        var pawn = Accessor.ChoiceLetter_BabyToChild.Pawn(letter);
        var bornSlave = pawn!.IsSlave;

        if (bornSlave)
        {
            return ChoiceKind switch
            {
                LetterChoiceKind.Option1 => ChoiceWithName(choices, "Emancipate"),
                LetterChoiceKind.Option2 => ChoiceWithName(choices, "RemainX"),
                _ => null
            };
        }
        else
        {
            return ChoiceKind switch
            {
                LetterChoiceKind.Option1 => ChoiceWithName(choices, "RemainX"),
                LetterChoiceKind.Option2 => ChoiceWithName(choices, "Enslave"),
                _ => null
            };
        }
    }

    protected override DiaOption ChoiceWithName(List<DiaOption> choices, string translateKey)
    {
        var textToMatch = translateKey.Translate().CapitalizeFirst();
        return choices.FirstOrDefault(c => Accessor.DiaOption.Text(c) == textToMatch);
    }
}
