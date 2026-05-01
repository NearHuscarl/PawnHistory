using System;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.DebugTools;
using RimWorld;
using Verse;

namespace PawnHistory.Source.PawnTracker.Test;

public class LetterActionSimple<TLetter> : LetterAction<TLetter> where TLetter : ChoiceLetter
{
    private LetterChoiceKind choiceKind;

    public LetterActionSimple<TLetter> Accept()
    {
        choiceKind = LetterChoiceKind.Accept;
        return this;
    }
    public LetterActionSimple<TLetter> Reject()
    {
        choiceKind = LetterChoiceKind.Reject;
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

        var choice = ResolveChoice(choiceLetter, choiceKind);

        if (choice == null)
            throw new InvalidOperationException($"{choiceKind} option not found in {typeof(TLetter).Name}: {DebugUtility.Format(choiceLetter.Choices)}.");

        if (!choice.disabledReason.NullOrEmpty())
            throw new InvalidOperationException($"{choiceKind} option is disabled in {typeof(TLetter).Name}: {choice.disabledReason}");

        choice.action.Invoke();
    }

    private static DiaOption ResolveChoice(ChoiceLetter letter, LetterChoiceKind choiceKind)
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