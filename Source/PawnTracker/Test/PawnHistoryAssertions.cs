using PawnHistory.Source.Helper;
using System;
using System.Linq;
using System.Security.Cryptography;
using Verse;
using static UnityEngine.Networking.UnityWebRequest;

namespace PawnHistory.Source.PawnTracker.Test;

public sealed class PawnHistoryAssertions(Pawn pawn)
{
    private readonly Pawn pawn = pawn ?? throw new ArgumentNullException(nameof(pawn));

    private bool isEventually;
    private int eventuallyTimeoutTicks;
    private int eventuallyPollIntervalTicks;
    private bool negate = false;

    public PawnHistoryAssertions Not()
    {
        negate = !negate;
        return this;
    }

    private void AssertCondition(bool condition, string positiveMessage, string negativeMessage)
    {
        if (negate ? condition : !condition)
            TestManager.Ctx.Fail(negate ? negativeMessage : positiveMessage);
    }

    public PawnHistoryAssertions ToHaveHistoryRecordOf(HistoryRecordDef def)
    {
        RunAssertion(() =>
        {
            var hasRecord = pawn.GetHistoryRecords().Any(r => r.def == def);
            AssertCondition(
                hasRecord,
                $"Expected record of type '{def.defName}' for {pawn} but none found.",
                $"Expected NO record of type '{def.defName}' for {pawn} but one was found."
            );
        });

        return this;
    }

    public PawnHistoryAssertions ToHaveHistoryRecordCount(int expected)
    {
        RunAssertion(() =>
        {
            var actual = pawn.GetHistoryRecords().Count;
            var result = actual != expected;

            AssertCondition(
                result,
                $"Expected {expected} number of records but got {actual}.",
                $"Expected NOT {expected} number of records but got {actual}."
            );
        });

        return this;
    }

    public PawnHistoryAssertions ToHaveHistoryRecord(string descriptionTemplate, int index = -1, bool exactMatch = false)
    {
        RunAssertion(() =>
        {
            var lastRecord = pawn.GetHistoryRecords().At(index);
            var actual = lastRecord.description.StripTags();
            var isTheSame = LangUtility.IsStructurallyTheSame(descriptionTemplate, actual, exactMatch);

            AssertCondition(
                isTheSame,
                $"Expected description to match template\nExpected template [exactMatch={exactMatch}]:\n{descriptionTemplate}\nActual resolved description:\n{actual}",
                $"Expected description NOT to match template\nExpected template [exactMatch={exactMatch}]:\n{descriptionTemplate}\nActual resolved description:\n{actual}."
            );
        });

        return this;
    }

    public PawnHistoryAssertions Eventually(int timeoutTicks = 3000, int pollIntervalTicks = 25)
    {
        isEventually = true;
        eventuallyTimeoutTicks = timeoutTicks;
        eventuallyPollIntervalTicks = pollIntervalTicks;
        return this;
    }

    private void RunAssertion(Action assertion)
    {
        var ctx = TestManager.Ctx;

        if (!isEventually)
        {
            try
            {
                assertion();
                ctx.AssertionsPassed++;
            }
            catch (Exception ex)
            {
                ctx.AssertionsFailed++;
                throw ex;
            }
            return;
        }

        var tickStart = Find.TickManager.TicksGame;
        Exception lastException = null;

        ctx.PendingEventually++;

        var action = TickDelayManager.Interval(eventuallyPollIntervalTicks, (a) =>
        {
            if (Find.TickManager.TicksGame - tickStart > eventuallyTimeoutTicks)
            {
                ctx.PendingEventually--;
                ctx.AssertionsFailed++;
                a.Cancelled = true;
                ctx.Fail(lastException, $"Test assertion failed after {eventuallyTimeoutTicks} ticks.");
            }

            try
            {
                assertion();
                ctx.PendingEventually--;
                ctx.AssertionsPassed++;
                a.Cancelled = true;
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        });

        ctx.OnCleanup(() => action.Data.Cancelled = true);
        // reset mode so next assertions are normal unless re-enabled
        isEventually = false;
    }
}