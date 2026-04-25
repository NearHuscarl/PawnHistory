using System;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.DebugTools;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

internal enum LetterChoiceKind
{
    Accept,
    Reject
}

public sealed class LetterAction<TLetter> where TLetter : ChoiceLetter
{
    private readonly List<Func<TLetter, bool>> filters = [];
    private TLetter letter;

    public LetterAction<TLetter> Filter(Func<TLetter, bool> predicate)
    {
        filters.Add(predicate);
        return this;
    }

    public TLetter Accept()
    {
        return Execute(LetterChoiceKind.Accept);
    }

    public TLetter Reject()
    {
        return Execute(LetterChoiceKind.Reject);
    }

    private TLetter Execute(LetterChoiceKind choiceKind)
    {
        letter = Find.LetterStack.LettersListForReading
            .OfType<TLetter>()
            .Reverse()
            .FirstOrDefault(l => filters.All(f => f(l)));

        if (letter == null)
            throw new InvalidOperationException($"No active letter of type {typeof(TLetter).Name} was found.");

        Choose(choiceKind);
        return letter;
    }

    private void Choose(LetterChoiceKind choiceKind)
    {
        var choice = ResolveChoice(letter, choiceKind);

        if (choice == null)
            throw new InvalidOperationException($"{choiceKind} option not found in {typeof(TLetter).Name}: {DebugUtility.Format(letter.Choices)}.");

        if (!choice.disabledReason.NullOrEmpty())
            throw new InvalidOperationException($"{choiceKind} option is disabled in {typeof(TLetter).Name}: {choice.disabledReason}");

        choice.action.Invoke();
    }

    private static DiaOption ResolveChoice(TLetter letter, LetterChoiceKind choiceKind)
    {
        var choices = letter.Choices.ToList();

        if (typeof(TLetter) == typeof(ChoiceLetter_AcceptJoiner))
            return choiceKind switch
            {
                LetterChoiceKind.Accept => ChoiceWithName(choices, "AcceptButton"),
                LetterChoiceKind.Reject => ChoiceWithName(choices, "RejectLetter"),
                _ => null
            };

        if (typeof(TLetter) == typeof(ChoiceLetter_RansomDemand))
            return choiceKind switch
            {
                LetterChoiceKind.Accept => ChoiceWithName(choices, "RansomDemand_Accept"),
                LetterChoiceKind.Reject => ChoiceWithName(choices, "RejectLetter"),
                _ => null
            };

        if (typeof(TLetter) == typeof(ChoiceLetter_AcceptVisitors))
            return choiceKind switch
            {
                LetterChoiceKind.Accept => ChoiceWithName(choices, "AcceptButton"),
                LetterChoiceKind.Reject => ChoiceWithName(choices, "RejectLetter"), // TODO: have a confirm dialog
                _ => null
            };

        return null;
    }

    private static DiaOption ChoiceWithName(List<DiaOption> choices, string translateKey)
    {
        var textToMatch = translateKey.Translate();
        return choices.FirstOrDefault(c => Accessor.DiaOption.Text(c) == textToMatch);
    }
}
