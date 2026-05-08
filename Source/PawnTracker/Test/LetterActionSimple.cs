using System;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.DebugTools;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class LetterActionSimple<TLetter> : LetterAction<TLetter> where TLetter : ChoiceLetter
{
    protected LetterChoiceKind ChoiceKind;

    public LetterActionSimple<TLetter> Accept()
    {
        return SetChoice(LetterChoiceKind.Accept);
    }
    public LetterActionSimple<TLetter> Reject()
    {
        return SetChoice(LetterChoiceKind.Reject);
    }

    protected LetterActionSimple<TLetter> SetChoice(LetterChoiceKind kind)
    {
        ChoiceKind = kind;
        return this;
    }

    public override TLetter Execute()
    {
        base.Execute();
        Choose();
        return Letter;
    }

    private void Choose()
    {
        if (Letter is not ChoiceLetter choiceLetter)
            throw new InvalidOperationException($"{typeof(TLetter).Name} does not support accept/reject choices.");

        var choice = ResolveChoice(choiceLetter as TLetter);

        if (choice == null)
            throw new InvalidOperationException($"{ChoiceKind} option not found in {typeof(TLetter).Name}: {DebugUtility.Format(choiceLetter.Choices)}.");

        if (choice.disabled)
            throw new InvalidOperationException($"{ChoiceKind} option is disabled in {typeof(TLetter).Name}: {choice.disabledReason}");

        choice.action.Invoke();
    }

    protected virtual DiaOption ResolveChoice(TLetter letter)
    {
        var choices = letter.Choices.ToList();

        if (typeof(TLetter) == typeof(ChoiceLetter_AcceptJoiner))
            return ChoiceKind switch
            {
                LetterChoiceKind.Accept => ChoiceWithName(choices, "AcceptButton"),
                LetterChoiceKind.Reject => ChoiceWithName(choices, "RejectLetter"),
                _ => null
            };

        if (typeof(TLetter) == typeof(ChoiceLetter_RansomDemand))
            return ChoiceKind switch
            {
                LetterChoiceKind.Accept => ChoiceWithName(choices, "RansomDemand_Accept"),
                LetterChoiceKind.Reject => ChoiceWithName(choices, "RejectLetter"),
                _ => null
            };

        if (typeof(TLetter) == typeof(ChoiceLetter_AcceptVisitors))
            return ChoiceKind switch
            {
                LetterChoiceKind.Accept => ChoiceWithName(choices, "AcceptButton"),
                LetterChoiceKind.Reject => ChoiceWithName(choices, "RejectLetter"), // TODO: have a confirm dialog
                _ => null
            };

        return null;
    }

    protected virtual DiaOption ChoiceWithName(List<DiaOption> choices, string translateKey)
    {
        var textToMatch = translateKey.Translate();
        return choices.FirstOrDefault(c => Accessor.DiaOption.Text(c) == textToMatch);
    }
}
