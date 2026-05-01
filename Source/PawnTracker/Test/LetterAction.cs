using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

internal enum LetterChoiceKind
{
    Accept,
    Reject
}

public abstract class LetterAction<TLetter> where TLetter : Letter
{
    protected TLetter Letter;
    private readonly List<Func<TLetter, bool>> filters = [];

    public LetterAction<TLetter> Filter(Func<TLetter, bool> predicate)
    {
        filters.Add(predicate);
        return this;
    }

    private TLetter ResolveLetter()
    {
        Letter = Find.LetterStack.LettersListForReading
            .OfType<TLetter>()
            .Reverse()
            .FirstOrDefault(l => filters.All(f => f(l)));

        if (Letter == null)
            throw new InvalidOperationException($"No active letter of type {typeof(TLetter).Name} was found.");

        return Letter;
    }

    public virtual TLetter Execute()
    {
        Letter = ResolveLetter();
        return Letter;
    }
}
